import uuid

from sqlalchemy import ForeignKey
from sqlalchemy.dialects.postgresql import ENUM as PG_ENUM
from sqlalchemy.dialects.postgresql import UUID as PG_UUID
from sqlalchemy.orm import Mapped, mapped_column, relationship

from minigram_core.db.base import BaseModel
from minigram_profile.models.profile import Profile
from minigram_profile.models.status import tStatus


class Relation(BaseModel):
    __tablename__ = "relations"

    sender_id: Mapped[uuid.UUID] = mapped_column(
        "senderid",
        PG_UUID(as_uuid=True),
        ForeignKey("profiles.id", ondelete="CASCADE"),
        nullable=False,
    )
    receiver_id: Mapped[uuid.UUID] = mapped_column(
        "receiverid",
        PG_UUID(as_uuid=True),
        ForeignKey("profiles.id", ondelete="CASCADE"),
        nullable=False,
    )
    status: Mapped[tStatus] = mapped_column(
        "status",
        PG_ENUM(tStatus, name="tstatus", values_callable=lambda e: [v.value for v in e], create_type=False),
        nullable=False,
        default=tStatus.none,
    )

    sender: Mapped[Profile] = relationship("Profile", foreign_keys=[sender_id], lazy="joined")
    receiver: Mapped[Profile] = relationship("Profile", foreign_keys=[receiver_id], lazy="joined")
