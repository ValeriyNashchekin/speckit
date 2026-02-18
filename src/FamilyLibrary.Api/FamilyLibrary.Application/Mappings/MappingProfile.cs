using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Entities;
using Mapster;

namespace FamilyLibrary.Application.Mappings;

public static class MappingProfile
{
    public static void ConfigureMappings()
    {
        // FamilyRole mappings
        TypeAdapterConfig<FamilyRoleEntity, FamilyRoleDto>
            .NewConfig()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        TypeAdapterConfig<CreateFamilyRoleDto, FamilyRoleEntity>
            .NewConfig()
            .ConstructUsing(src => new FamilyRoleEntity(src.Name, src.Type, src.Description, src.CategoryId));

        // Category mappings
        TypeAdapterConfig<CategoryEntity, CategoryDto>
            .NewConfig();

        TypeAdapterConfig<CreateCategoryDto, CategoryEntity>
            .NewConfig()
            .ConstructUsing(src => new CategoryEntity(src.Name, src.Description, src.SortOrder));

        // Tag mappings
        TypeAdapterConfig<TagEntity, TagDto>
            .NewConfig();

        TypeAdapterConfig<CreateTagDto, TagEntity>
            .NewConfig()
            .ConstructUsing(src => new TagEntity(src.Name, src.Color));

        // Family mappings
        TypeAdapterConfig<FamilyEntity, FamilyDto>
            .NewConfig()
            .Map(dest => dest.RoleName, src => src.Role != null ? src.Role.Name : string.Empty)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        TypeAdapterConfig<FamilyEntity, FamilyDetailDto>
            .NewConfig()
            .Map(dest => dest.RoleName, src => src.Role != null ? src.Role.Name : string.Empty)
            .Map(dest => dest.Versions, src => src.Versions.ToList());

        TypeAdapterConfig<CreateFamilyDto, FamilyEntity>
            .NewConfig()
            .ConstructUsing(src => new FamilyEntity(src.RoleId, src.FamilyName));

        // FamilyVersion mappings
        TypeAdapterConfig<FamilyVersionEntity, FamilyVersionDto>
            .NewConfig()
            .Map(dest => dest.PublishedBy, src => src.PublishedBy);

        // SystemType mappings
        TypeAdapterConfig<SystemTypeEntity, SystemTypeDto>
            .NewConfig()
            .Map(dest => dest.RoleName, src => src.Role != null ? src.Role.Name : string.Empty)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        TypeAdapterConfig<CreateSystemTypeDto, SystemTypeEntity>
            .NewConfig()
            .ConstructUsing(src => new SystemTypeEntity(
                src.RoleId,
                src.TypeName,
                src.Category,
                src.SystemFamily,
                src.Group,
                src.Json,
                src.Hash));

        // Draft mappings
        TypeAdapterConfig<DraftEntity, DraftDto>
            .NewConfig()
            .Map(dest => dest.SelectedRoleName, src => src.SelectedRole != null ? src.SelectedRole.Name : null)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        TypeAdapterConfig<CreateDraftDto, DraftEntity>
            .NewConfig()
            .ConstructUsing(src => new DraftEntity(src.FamilyName, src.FamilyUniqueId, src.TemplateId));

        // RecognitionRule mappings
        TypeAdapterConfig<RecognitionRuleEntity, RecognitionRuleDto>
            .NewConfig();

        TypeAdapterConfig<CreateRecognitionRuleDto, RecognitionRuleEntity>
            .NewConfig()
            .ConstructUsing(src => new RecognitionRuleEntity(src.RoleId, src.RootNode, src.Formula));

        // MaterialMapping mappings
        TypeAdapterConfig<MaterialMappingEntity, MaterialMappingDto>
            .NewConfig();

        TypeAdapterConfig<CreateMaterialMappingRequest, MaterialMappingEntity>
            .NewConfig()
            .ConstructUsing(src => new MaterialMappingEntity(
                src.ProjectId,
                src.TemplateMaterialName,
                src.ProjectMaterialName));
    }
}
