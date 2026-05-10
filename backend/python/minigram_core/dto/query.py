from dataclasses import dataclass
from typing import Annotated

from fastapi import Query


@dataclass
class QueryParams:
    page: Annotated[int | None, Query()] = None
    per_page: Annotated[int | None, Query(alias="perPage")] = None
