from enum import Enum
from typing import Annotated

from pydantic import BeforeValidator


class tStatus(str, Enum):
    none = "none"
    friend = "friend"
    blocked = "blocked"


_INT_TO_STATUS = {0: tStatus.none.value, 1: tStatus.friend.value, 2: tStatus.blocked.value}
_LOWER_TO_STATUS = {m.value.lower(): m.value for m in tStatus}


def _coerce_status(value: object) -> object:
    if isinstance(value, int) and not isinstance(value, bool):
        return _INT_TO_STATUS.get(value, value)
    if isinstance(value, str):
        stripped = value.strip()
        if stripped.lstrip("-").isdigit():
            return _INT_TO_STATUS.get(int(stripped), value)
        return _LOWER_TO_STATUS.get(stripped.lower(), value)
    return value


StatusQuery = Annotated[tStatus, BeforeValidator(_coerce_status)]
