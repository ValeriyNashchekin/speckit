// Auto-generated from contracts/webview-events.md
// DO NOT EDIT MANUALLY

/**
 * Base event structure for all WebView2 communications
 */
export interface WebViewEvent<T = unknown> {
  type: string;
  payload: T;
  timestamp?: number;
  correlationId?: string;
}

// ==================== PLUGIN -> FRONTEND EVENTS ====================

/**
 * Plugin loaded and ready to receive commands
 */
export interface RevitReadyPayload {
  version: string;
  pluginVersion: string;
  documentType: 'Project' | 'Family' | 'None';
  documentPath?: string;
}

/**
 * Family item from document scan
 */
export interface FamilyItem {
  id: string;
  name: string;
  categoryName: string;
  isSystemFamily: boolean;
  hasStamp: boolean;
  stampData?: StampData;
}

/**
 * ES stamp data attached to family
 */
export interface StampData {
  roleId: string;
  roleName: string;
  stampedAt: string;
  stampedBy: string;
  contentHash: string;
}

/**
 * List of families from current document scan
 */
export interface RevitFamiliesListPayload {
  families: FamilyItem[];
  scanDuration: number;
  projectId: string;
}

/**
 * Scan completed with summary
 */
export interface RevitScanCompletePayload {
  totalFamilies: number;
  loadableCount: number;
  systemCount: number;
  stampedCount: number;
  scanDuration: number;
}

/**
 * Result of stamp operation
 */
export interface RevitStampResultPayload {
  success: boolean;
  familyId: string;
  familyName: string;
  roleId: string;
  roleName: string;
  error?: string;
}

/**
 * Result of publish operation
 */
export interface RevitPublishResultPayload {
  success: boolean;
  familyId?: string;
  familyName: string;
  version?: number;
  error?: string;
}

/**
 * Result of load family operation
 */
export interface RevitLoadResultPayload {
  success: boolean;
  familyId: string;
  familyName: string;
  loadedSymbols?: string[];
  error?: string;
}

/**
 * Type catalog entry for selection dialog
 */
export interface TypeCatalogEntry {
  typeName: string;
  parameters: Record<string, string | number>;
}

/**
 * Request to show type selection dialog
 */
export interface RevitTypeCatalogShowPayload {
  familyId: string;
  familyName: string;
  types: TypeCatalogEntry[];
  parameterHeaders: string[];
}

/**
 * Generic error from Plugin
 */
export interface RevitErrorPayload {
  code: string;
  message: string;
  details?: string;
  recoverable: boolean;
}

// ==================== FRONTEND -> PLUGIN EVENTS ====================

/**
 * Frontend loaded and ready
 */
export interface UiReadyPayload {
  version: string;
  locale: string;
}

/**
 * Request to scan families in current document
 */
export interface UiScanFamiliesPayload {
  includeSystemFamilies?: boolean;
  groupFilter?: SystemFamilyGroup[];
}

export type SystemFamilyGroup = 'A' | 'B' | 'C' | 'D' | 'E';

/**
 * Stamp a family with role
 */
export interface UiStampPayload {
  familyId: string;
  familyName: string;
  roleId: string;
  roleName: string;
}

/**
 * Publish family to library
 */
export interface UiPublishPayload {
  familyId: string;
  familyName: string;
  commitMessage?: string;
  catalogFile?: string;
}

/**
 * Load family from library to project
 */
export interface UiLoadFamilyPayload {
  serverFamilyId: string;
  familyName: string;
  version?: number;
  targetSymbolNames?: string[];
}

/**
 * User selected types from catalog
 */
export interface UiTypeCatalogSelectPayload {
  familyId: string;
  selectedTypes: string[];
}

/**
 * Navigate to URL in WebView2
 */
export interface UiNavigatePayload {
  url: string;
}

/**
 * Log message from Frontend
 */
export interface UiLogPayload {
  level: 'debug' | 'info' | 'warn' | 'error';
  message: string;
  data?: unknown;
}

// ==================== EVENT TYPE CONSTANTS ====================

export const PluginEventTypes = {
  REVIT_READY: 'revit:ready',
  REVIT_FAMILIES_LIST: 'revit:families:list',
  REVIT_SCAN_COMPLETE: 'revit:scan:complete',
  REVIT_STAMP_RESULT: 'revit:stamp:result',
  REVIT_PUBLISH_RESULT: 'revit:publish:result',
  REVIT_LOAD_RESULT: 'revit:load:result',
  REVIT_TYPE_CATALOG_SHOW: 'revit:type-catalog:show',
  REVIT_ERROR: 'revit:error',
} as const;

export const UiEventTypes = {
  UI_READY: 'ui:ready',
  UI_SCAN_FAMILIES: 'ui:scan-families',
  UI_STAMP: 'ui:stamp',
  UI_PUBLISH: 'ui:publish',
  UI_LOAD_FAMILY: 'ui:load-family',
  UI_TYPE_CATALOG_SELECT: 'ui:type-catalog:select',
  UI_NAVIGATE: 'ui:navigate',
  UI_LOG: 'ui:log',
} as const;

// ==================== PHASE 2: SCANNER & UPDATE EVENTS ====================
// Phase 2 additions for family library update detection

import type {
  ScanResult,
  ChangeSet,
  FamilyScanStatus,
} from './scanner.models';

/**
 * Plugin -> Frontend: Scan result with all families
 */
export interface ScanResultEvent {
  type: 'revit:scan:result';
  payload: ScanResult;
}

/**
 * Plugin -> Frontend: Scan progress update
 */
export interface ScanProgressEvent {
  type: 'revit:scan:progress';
  payload: {
    scanned: number;
    total: number;
    currentFamily: string;
  };
}

/**
 * Plugin -> Frontend: Family update progress
 */
export interface UpdateProgressEvent {
  type: 'revit:update:progress';
  payload: {
    completed: number;
    total: number;
    currentFamily: string;
    success: number;
    failed: number;
  };
}

/**
 * Plugin -> Frontend: Update operation completed
 */
export interface UpdateCompleteEvent {
  type: 'revit:update:complete';
  payload: {
    total: number;
    success: number;
    failed: number;
    errors: Array<{
      familyName: string;
      error: string;
    }>;
  };
}

/**
 * Plugin -> Frontend: Changes detected for a family
 */
export interface ChangesResultEvent {
  type: 'revit:changes:result';
  payload: {
    familyUniqueId: string;
    changes: ChangeSet;
  };
}

/**
 * UI -> Plugin: Request project scan
 */
export interface ScanProjectEvent {
  type: 'ui:scan-project';
  payload: {
    includeSystemFamilies: boolean;
  };
}

/**
 * UI -> Plugin: Request family updates
 */
export interface UpdateFamiliesEvent {
  type: 'ui:update-families';
  payload: {
    families: Array<{
      uniqueId: string;
      roleName?: string;
    }>;
    showPreview: boolean;
  };
}

/**
 * UI -> Plugin: Stamp families with legacy roles
 */
export interface StampLegacyEvent {
  type: 'ui:stamp-legacy';
  payload: {
    families: Array<{
      uniqueId: string;
      roleName: string;
    }>;
  };
}

/**
 * UI -> Plugin: Request changes detail for a family
 */
export interface GetChangesEvent {
  type: 'ui:get-changes';
  payload: {
    uniqueId: string;
  };
}

// Phase 2 Event Type Constants
export const Phase2PluginEventTypes = {
  REVIT_SCAN_RESULT: 'revit:scan:result',
  REVIT_SCAN_PROGRESS: 'revit:scan:progress',
  REVIT_UPDATE_PROGRESS: 'revit:update:progress',
  REVIT_UPDATE_COMPLETE: 'revit:update:complete',
  REVIT_CHANGES_RESULT: 'revit:changes:result',
} as const;

export const Phase2UiEventTypes = {
  UI_SCAN_PROJECT: 'ui:scan-project',
  UI_UPDATE_FAMILIES: 'ui:update-families',
  UI_STAMP_LEGACY: 'ui:stamp-legacy',
  UI_GET_CHANGES: 'ui:get-changes',
} as const;
