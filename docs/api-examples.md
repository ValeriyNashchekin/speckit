# API Usage Examples

This document provides practical examples for using the Family Library API.

---

## Base URL

```
Development: http://localhost:5000/api
Production: https://family-library.freeaxez.com/api
```

## Authentication

MVP uses mock authentication for development. Production will use Azure AD.

```http
# Development - no auth required
GET /api/roles

# Production - Bearer token required
Authorization: Bearer <your-jwt-token>
```

---

## Roles API

### Get All Roles

```http
GET /api/roles?page=1&pageSize=10
```

Response:
```json
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "FreeAxez_Table",
      "type": "Loadable",
      "description": "Table families",
      "category": {
        "id": "category-guid",
        "name": "Furniture"
      },
      "tags": [
        { "id": "tag-guid", "name": "Office", "color": "#3498db" }
      ],
      "createdAt": "2026-02-17T10:30:00Z",
      "updatedAt": "2026-02-17T10:30:00Z"
    }
  ]
}
```

### Get Roles by Type

```http
GET /api/roles?type=Loadable
GET /api/roles?type=System
```

### Get Single Role

```http
GET /api/roles/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Create Single Role

```http
POST /api/roles
Content-Type: application/json

{
  "name": "FreeAxez_Window",
  "type": "Loadable",
  "description": "Window families"
}
```

Response: `201 Created`

### Batch Create Roles

```http
POST /api/roles
Content-Type: application/json

{
  "roles": [
    { "name": "FreeAxez_Door", "type": "Loadable", "description": "Doors" },
    { "name": "FreeAxez_Window", "type": "Loadable", "description": "Windows" },
    { "name": "FreeAxez_Wall", "type": "System", "description": "Wall types" }
  ]
}
```

### Update Role

Only description, categoryId, and tagIds can be updated. Name and type are read-only.

```http
PUT /api/roles/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "description": "Updated description",
  "categoryId": "new-category-guid",
  "tagIds": ["tag-1-guid", "tag-2-guid"]
}
```

### Delete Role

```http
DELETE /api/roles/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Response: `204 No Content`

---

## Families API

### Search Families

```http
GET /api/families?search=door&roleId=role-guid&page=1&pageSize=20
```

### Get Family Details

```http
GET /api/families/family-guid
```

### Publish Family

```http
POST /api/families/publish
Content-Type: multipart/form-data

rfaFile: (binary .rfa file)
txtFile: (optional binary .txt file)
metadata: { "roleId": "role-guid", "familyName": "Door_Single_Flush" }
```

### Download Family

```http
GET /api/families/family-guid/download/2
```

### Batch Check Family Statuses

```http
POST /api/families/batch-check
Content-Type: application/json

{
  "families": [
    { "roleName": "FreeAxez_Door", "contentHash": "hash-1" },
    { "roleName": "FreeAxez_Window", "contentHash": "hash-2" }
  ]
}
```

Status values: UpToDate, UpdateAvailable, LegacyMatch, NotInLibrary

---

## System Types API

### Get System Types

```http
GET /api/system-types?roleId=role-guid&group=A&page=1&pageSize=50
```

### Create/Update System Type

```http
POST /api/system-types
Content-Type: application/json

{
  "roleId": "role-guid",
  "typeName": "Wall_External_200",
  "category": "Walls",
  "systemFamily": "Basic Wall",
  "group": "A",
  "json": {},
  "hash": "sha256-hash"
}
```

---

## Categories API

### Get All Categories

```http
GET /api/categories
```

### Create Category

```http
POST /api/categories
Content-Type: application/json

{
  "name": "Structural",
  "description": "Structural elements",
  "sortOrder": 3
}
```

---

## Tags API

### Get All Tags

```http
GET /api/tags
```

### Create Tag

```http
POST /api/tags
Content-Type: application/json

{
  "name": "Industrial",
  "color": "#9b59b6"
}
```

---

## Recognition Rules API

### Get Recognition Rules

```http
GET /api/recognition-rules
```

### Create Recognition Rule

```http
POST /api/recognition-rules
Content-Type: application/json

{
  "roleId": "role-guid",
  "formula": "(FB OR Desk) AND Wired",
  "rootNode": {
    "type": "group",
    "operator": "AND",
    "children": [...]
  }
}
```

### Validate Formula Syntax

```http
POST /api/recognition-rules/validate
Content-Type: application/json

{
  "formula": "Door AND (Window OR Wall"
}
```

### Test Rule Against Family Name

```http
POST /api/recognition-rules/test
Content-Type: application/json

{
  "roleId": "role-guid",
  "testName": "FB_Field_Wired_v2"
}
```

---

## Drafts API

### Get Drafts by Template

```http
GET /api/drafts?templateId=template-guid&status=New
```

Status values: New, RoleSelected, Stamped, Published

### Create Draft

```http
POST /api/drafts
Content-Type: application/json

{
  "familyName": "New_Family",
  "familyUniqueId": "Revit-UniqueId",
  "templateId": "template-guid"
}
```

### Update Draft

```http
PUT /api/drafts/draft-guid
Content-Type: application/json

{
  "selectedRoleId": "role-guid",
  "status": "RoleSelected",
  "contentHash": "sha256-hash"
}
```

### Delete Draft

```http
DELETE /api/drafts/draft-guid
```

---

## Error Responses

### HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 404 | Not Found |
| 409 | Conflict |
| 500 | Internal Server Error |

---

## Using with cURL

### Get Roles

```bash
curl -X GET "http://localhost:5000/api/roles?page=1&pageSize=10" -H "accept: application/json"
```

### Create Role

```bash
curl -X POST "http://localhost:5000/api/roles" -H "Content-Type: application/json" -d '{"name":"FreeAxez_Door","type":"Loadable"}'
```

---

## Next Steps

- Review the [Quick Start Guide](./quickstart.md) for setup instructions
- Check the [API Specification](../specs/001-family-library-mvp/contracts/api.yaml) for OpenAPI schema
- Read the [Feature Specification](../specs/001-family-library-mvp/spec.md) for business requirements
