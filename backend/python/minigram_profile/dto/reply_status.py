from enum import Enum


class tReplyStatus(str, Enum):
    Accepted = "Accepted"
    Rejected = "Rejected"
    Blocked = "Blocked"
