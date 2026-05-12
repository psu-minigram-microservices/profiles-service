from enum import Enum
from typing import Annotated

from pydantic import BeforeValidator


class tRelationType(str, Enum):
    Incoming = "Incoming"
    Outgoing = "Outgoing"


_INT_TO_RELATION_TYPE = {0: tRelationType.Incoming.value, 1: tRelationType.Outgoing.value}
_LOWER_TO_RELATION_TYPE = {m.value.lower(): m.value for m in tRelationType}


def _coerce_relation_type(value: object) -> object:
    if isinstance(value, int) and not isinstance(value, bool):
        return _INT_TO_RELATION_TYPE.get(value, value)
    if isinstance(value, str):
        stripped = value.strip()
        if stripped.lstrip("-").isdigit():
            return _INT_TO_RELATION_TYPE.get(int(stripped), value)
        return _LOWER_TO_RELATION_TYPE.get(stripped.lower(), value)
    return value


RelationTypeQuery = Annotated[tRelationType, BeforeValidator(_coerce_relation_type)]
