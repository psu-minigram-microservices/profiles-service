from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class JwtSettings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    secret: str = Field(..., min_length=1, validation_alias="JWT__Secret")
    audience: str = Field(..., min_length=1, validation_alias="JWT__Audience")
    issuer: str = Field(..., min_length=1, validation_alias="JWT__Issuer")


class DatabaseSettings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    default_connection: str = Field(..., validation_alias="ConnectionStrings__DefaultConnection")

    @property
    def url(self) -> str:
        return _convert_dotnet_connection_string(self.default_connection)


class AppSettings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    environment: str = Field(default="Production", validation_alias="ASPNETCORE_ENVIRONMENT")
    urls: str | None = Field(default=None, validation_alias="ASPNETCORE_URLS")

    jwt: JwtSettings = Field(default_factory=JwtSettings)
    db: DatabaseSettings = Field(default_factory=DatabaseSettings)

    @property
    def is_development(self) -> bool:
        return self.environment.lower() == "development"


def _convert_dotnet_connection_string(value: str) -> str:
    parts: dict[str, str] = {}
    for piece in value.split(";"):
        piece = piece.strip()
        if not piece or "=" not in piece:
            continue
        key, _, val = piece.partition("=")
        parts[key.strip().lower()] = val.strip()

    host = parts.get("host", "localhost")
    port = parts.get("port", "5432")
    database = parts.get("database", "")
    username = parts.get("username", "")
    password = parts.get("password", "")

    return f"postgresql+asyncpg://{username}:{password}@{host}:{port}/{database}"


@lru_cache(maxsize=1)
def get_settings() -> AppSettings:
    return AppSettings()
