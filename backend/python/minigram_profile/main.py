import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI

from minigram_core.db.session import Database
from minigram_core.middleware.exception_handler import register_exception_handlers
from minigram_profile.config import get_settings
from minigram_profile.routers import profile_router, relation_router

logging.basicConfig(level=logging.INFO)


@asynccontextmanager
async def lifespan(app: FastAPI):
    settings = get_settings()
    app.state.database = Database(settings.db.url)
    try:
        yield
    finally:
        await app.state.database.dispose()


def create_app() -> FastAPI:
    settings = get_settings()

    app = FastAPI(
        title="Minigram.Profile API",
        version="v1",
        openapi_url="/swagger/v1/swagger.json" if settings.is_development else None,
        docs_url="/" if settings.is_development else None,
        redoc_url=None,
        lifespan=lifespan,
    )

    api_v1_prefix = "/api/v1"
    app.include_router(relation_router, prefix=api_v1_prefix)
    app.include_router(profile_router, prefix=api_v1_prefix)

    register_exception_handlers(app, is_development=settings.is_development)

    return app


app = create_app()
