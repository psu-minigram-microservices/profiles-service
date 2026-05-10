import uuid
from datetime import datetime

from sqlalchemy import DateTime
from sqlalchemy.dialects.postgresql import UUID as PG_UUID
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column


class BaseModel(DeclarativeBase):
    __abstract__ = True

    id: Mapped[uuid.UUID] = mapped_column("id", PG_UUID(as_uuid=True), primary_key=True)
    created_at: Mapped[datetime | None] = mapped_column("createdat", DateTime, nullable=True)
    updated_at: Mapped[datetime | None] = mapped_column("updatedat", DateTime, nullable=True)
