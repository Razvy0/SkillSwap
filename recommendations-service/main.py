import os
from typing import List

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
import google.generativeai as genai
import psycopg
from pgvector.psycopg import register_vector
from dotenv import load_dotenv

load_dotenv()  # This loads the variables from the .env file into os.getenv

app = FastAPI(title="SkillSync Recommendations Service")


class RecommendationRequest(BaseModel):
    user_id: str
    profile_text: str
    top_k: int = Field(default=3, ge=1, le=10)


class RecommendationMatch(BaseModel):
    user_id: str
    similarity: float
    reason: str


class RecommendationResponse(BaseModel):
    matches: List[RecommendationMatch]


def _get_required_env(name: str) -> str:
    value = os.getenv(name)
    if not value:
        raise RuntimeError(f"Missing required environment variable: {name}")
    return value


def _configure_genai() -> None:
    api_key = _get_required_env("GEMINI_API_KEY")
    genai.configure(api_key=api_key)


def _get_db_connection():
    connection_string = _get_required_env("CONNECTION_STRING")
    conn = psycopg.connect(connection_string)
    register_vector(conn)
    return conn


def _embed_text(text: str) -> List[float]:
    model = os.getenv("GEMINI_EMBEDDING_MODEL", "models/gemini-embedding-001")
    result = genai.embed_content(
        model=model,
        content=text,
        task_type="RETRIEVAL_DOCUMENT",
        output_dimensionality=768  # 👈 This forces it to fit your pgvector table!
    )
    return result["embedding"]


def _generate_reason(user_text: str, candidate_text: str) -> str:
    model_name = os.getenv("GEMINI_REASON_MODEL", "gemini-1.5-flash")
    model = genai.GenerativeModel(model_name)
    prompt = (
        "You are matching two users for a skill swap lesson. "
        "Give one short sentence explaining why they match. "
        "Keep it friendly and specific.\n\n"
        f"Current user profile:\n{user_text}\n\n"
        f"Candidate profile:\n{candidate_text}\n"
    )
    response = model.generate_content(prompt)
    text = response.text.strip() if response.text else "Strong overlap in skills and goals."
    return text


@app.on_event("startup")
def on_startup():
    _configure_genai()


@app.get("/health")
def health_check():
    return {"status": "ok"}


@app.post("/recommendations", response_model=RecommendationResponse)
def recommendations(request: RecommendationRequest):
    if not request.profile_text.strip():
        raise HTTPException(status_code=400, detail="profile_text is required")

    try:
        embedding = _embed_text(request.profile_text)
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Embedding failed: {exc}")

    try:
        with _get_db_connection() as conn:
            with conn.cursor() as cur:
                model_name = os.getenv("GEMINI_EMBEDDING_MODEL", "models/gemini-embedding-001")
                cur.execute(
                    """
                    INSERT INTO user_embeddings (user_id, profile_text, embedding, model, updated_at)
                    VALUES (%s, %s, %s, %s, NOW())
                    ON CONFLICT (user_id) DO UPDATE
                    SET profile_text = EXCLUDED.profile_text,
                        embedding = EXCLUDED.embedding,
                        model = EXCLUDED.model,
                        updated_at = NOW();
                    """,
                    (request.user_id, request.profile_text, embedding, model_name)
                )

                cur.execute(
                    """
                    SELECT user_id, profile_text,
                           1 - (embedding <=> %s::vector) AS similarity
                    FROM user_embeddings
                    WHERE user_id <> %s
                    ORDER BY embedding <=> %s::vector
                    LIMIT %s;
                    """,
                    (embedding, request.user_id, embedding, request.top_k)
                )
                rows = cur.fetchall()
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"Database error: {exc}")

    matches: List[RecommendationMatch] = []
    for user_id, profile_text, similarity in rows:
        try:
            reason = _generate_reason(request.profile_text, profile_text)
        except Exception:
            reason = "Strong overlap in skills and learning goals."

        matches.append(
            RecommendationMatch(
                user_id=user_id,
                similarity=float(similarity),
                reason=reason
            )
        )

    return RecommendationResponse(matches=matches)
