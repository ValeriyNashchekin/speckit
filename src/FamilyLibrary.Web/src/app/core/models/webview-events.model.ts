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
