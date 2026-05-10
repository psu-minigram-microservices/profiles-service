from minigram_profile.dto.profile_response import ProfileResponseDto
from minigram_profile.dto.relation_response import RelationResponseDto
from minigram_profile.models.profile import Profile
from minigram_profile.models.relation import Relation


def profile_to_dto(profile: Profile) -> ProfileResponseDto:
    return ProfileResponseDto(
        id=profile.id,
        name=profile.name,
        photo_url=profile.photo_url,
    )


def relation_to_dto(relation: Relation) -> RelationResponseDto:
    return RelationResponseDto(
        status=relation.status,
        profile=profile_to_dto(relation.receiver),
    )
