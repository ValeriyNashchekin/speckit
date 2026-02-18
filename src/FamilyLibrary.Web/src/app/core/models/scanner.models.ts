// Scanner models for Phase 2 - Family Library Update Detection

/**
 * Status of a scanned family compared to library version
 */
export type FamilyScanStatus =
  | 'UpToDate'
  | 'UpdateAvailable'
  | 'LegacyMatch'
  | 'Unmatched'
  | 'LocalModified';

/**
 * Category of change detected between local and library versions
 */
export type ChangeCategory =
  | 'Name'
  | 'Category'
  | 'Types'
  | 'Parameters'
  | 'Geometry'
  | 'Txt';

/**
 * Result of scanning a single family in the project
 */
export interface ScannedFamily {
  uniqueId: string;
  familyName: string;
  category: string;
  roleName?: string;
  isAutoRole: boolean;
  status: FamilyScanStatus;
  localVersion?: number;
  libraryVersion?: number;
  localHash?: string;
  libraryHash?: string;
}

/**
 * Result of scanning all families in a project
 */
export interface ScanResult {
  families: ScannedFamily[];
  totalCount: number;
  summary: ScanSummary;
}

/**
 * Summary counts by status
 */
export interface ScanSummary {
  upToDate: number;
  updateAvailable: number;
  legacyMatch: number;
  unmatched: number;
  localModified: number;
}

/**
 * Set of changes between local and library version
 */
export interface ChangeSet {
  items: ChangeItem[];
  hasChanges: boolean;
}

/**
 * Single change item with category and values
 */
export interface ChangeItem {
  category: ChangeCategory;
  previousValue?: string;
  currentValue?: string;
  addedItems?: string[];
  removedItems?: string[];
  parameterChanges?: ParameterChange[];
}

/**
 * Parameter-level change detail
 */
export interface ParameterChange {
  name: string;
  previousValue?: string;
  currentValue?: string;
}

/**
 * Request to check multiple families against library
 */
export interface BatchCheckRequest {
  families: FamilyCheckItem[];
}

/**
 * Single family check item
 */
export interface FamilyCheckItem {
  roleName: string;
  hash: string;
}

/**
 * Response from batch check operation
 */
export interface BatchCheckResponse {
  results: FamilyCheckResult[];
}

/**
 * Result for a single family from batch check
 */
export interface FamilyCheckResult {
  roleName: string;
  status: FamilyScanStatus;
  libraryVersion?: number;
  currentVersion?: number;
  libraryHash?: string;
}
