from enum import Enum
from typing import Annotated

from pydantic import BeforeValidator


class tReplyStatus(str, Enum):
    Accepted = "Accepted"
    Rejected = "Rejected"
    Blocked = "Blocked"


_INT_TO_REPLY_STATUS = {
    0: tReplyStatus.Accepted.value,
    1: tReplyStatus.Rejected.value,
    2: tReplyStatus.Blocked.value,
}
_LOWER_TO_REPLY_STATUS = {m.value.lower(): m.value for m in tReplyStatus}


def _coerce_reply_status(value: object) -> object:
    if isinstance(value, int) and not isinstance(value, bool):
        return _INT_TO_REPLY_STATUS.get(value, value)
    if isinstance(value, str):
        stripped = value.strip()
        if stripped.lstrip("-").isdigit():
            return _INT_TO_REPLY_STATUS.get(int(stripped), value)
        return _LOWER_TO_REPLY_STATUS.get(stripped.lower(), value)
    return value


ReplyStatusQuery = Annotated[tReplyStatus, BeforeValidator(_coerce_reply_status)]
