import uuid

from sqlalchemy import String, Text
from sqlalchemy.dialects.postgresql import UUID as PG_UUID
from sqlalchemy.orm import Mapped, mapped_column

from minigram_core.db.base import BaseModel


class Profile(BaseModel):
    __tablename__ = "profiles"

    user_id: Mapped[uuid.UUID] = mapped_column("userid", PG_UUID(as_uuid=True), nullable=False, unique=True)
    name: Mapped[str] = mapped_column("name", String(120), nullable=False)
    photo_url: Mapped[str | None] = mapped_column("photourl", Text, nullable=True)
