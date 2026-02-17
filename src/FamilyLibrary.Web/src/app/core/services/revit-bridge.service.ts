import { Injectable, signal, computed } from '@angular/core';
import { Observable, fromEvent, filter, map, Subject } from 'rxjs';
import {
  WebViewEvent,
  RevitReadyPayload,
  RevitFamiliesListPayload,
  RevitStampResultPayload,
  RevitPublishResultPayload,
  RevitLoadResultPayload,
  UiReadyPayload,
  UiScanFamiliesPayload,
  UiStampPayload,
  UiPublishPayload,
  UiLoadFamilyPayload,
  PluginEventTypes,
  UiEventTypes,
} from '../models/webview-events.model';

/**
 * Service for WebView2 communication between Angular and Revit Plugin.
 * Detects embedded mode and handles bidirectional messaging.
 */
@Injectable({ providedIn: 'root' })
export class RevitBridgeService {
  private readonly isEmbedded = this.checkEmbeddedMode();
  private readonly messageSubject = new Subject<WebViewEvent>();
  
  // Signals for reactive state
  readonly isRevitReady = signal(false);
  readonly revitVersion = signal<string | null>(null);
  readonly documentType = signal<'Project' | 'Family' | 'None'>('None');
  readonly isEmbeddedMode = computed(() => this.isEmbedded);

  constructor() {
    if (this.isEmbedded) {
      this.initializeEmbeddedMode();
    } else {
      this.initializeStandaloneMode();
    }
  }

  /**
   * Check if running inside Revit WebView2
   */
  private checkEmbeddedMode(): boolean {
    return (
      typeof window !== 'undefined' &&
      'chrome' in window &&
      'webview' in (window as any).chrome
    );
  }

  /**
   * Initialize for embedded mode (inside Revit)
   */
  private initializeEmbeddedMode(): void {
    // Listen for messages from Revit Plugin
    (window as any).chrome.webview.addEventListener('message', (event: any) => {
      const message = event.data as WebViewEvent;
      this.messageSubject.next(message);
      this.handleRevitMessage(message);
    });

    // Send UI ready event after initialization
    setTimeout(() => this.sendUiReady(), 100);
  }

  /**
   * Initialize for standalone mode (browser)
   */
  private initializeStandaloneMode(): void {
    console.log('[RevitBridge] Running in standalone mode');
    
    // Listen for custom events (for testing)
    fromEvent<CustomEvent>(window, 'revit-message')
      .pipe(map(e => e.detail as WebViewEvent))
      .subscribe(event => {
        this.messageSubject.next(event);
        this.handleRevitMessage(event);
      });
  }

  /**
   * Handle incoming messages from Revit
   */
  private handleRevitMessage(event: WebViewEvent): void {
    switch (event.type) {
      case PluginEventTypes.REVIT_READY:
        const readyPayload = event.payload as RevitReadyPayload;
        this.revitVersion.set(readyPayload.version);
        this.documentType.set(readyPayload.documentType);
        this.isRevitReady.set(true);
        console.log('[RevitBridge] Revit ready:', readyPayload);
        break;
    }
  }

  /**
   * Send event to Revit Plugin
   */
  send<T>(type: string, payload: T): void {
    const event: WebViewEvent<T> = {
      type,
      payload,
      timestamp: Date.now(),
    };

    if (this.isEmbedded) {
      (window as any).chrome.webview.postMessage(event);
    } else {
      console.log('[RevitBridge] Not embedded, logging event:', event);
    }
  }

  /**
   * Subscribe to events from Revit Plugin
   */
  on<T>(eventType: string): Observable<T> {
    return this.messageSubject.asObservable().pipe(
      filter(e => e.type === eventType),
      map(e => e.payload as T)
    );
  }

  // ==================== CONVENIENCE METHODS ====================

  /**
   * Send UI ready event
   */
  sendUiReady(): void {
    this.send<UiReadyPayload>(UiEventTypes.UI_READY, {
      version: '1.0.0',
      locale: navigator.language,
    });
  }

  /**
   * Request family scan
   */
  scanFamilies(options?: UiScanFamiliesPayload): void {
    this.send(UiEventTypes.UI_SCAN_FAMILIES, options ?? {});
  }

  /**
   * Stamp family with role
   */
  stampFamily(payload: UiStampPayload): void {
    this.send(UiEventTypes.UI_STAMP, payload);
  }

  /**
   * Publish family to library
   */
  publishFamily(payload: UiPublishPayload): void {
    this.send(UiEventTypes.UI_PUBLISH, payload);
  }

  /**
   * Load family from library
   */
  loadFamily(payload: UiLoadFamilyPayload): void {
    this.send(UiEventTypes.UI_LOAD_FAMILY, payload);
  }

  /**
   * Subscribe to families list events
   */
  onFamiliesList(): Observable<RevitFamiliesListPayload> {
    return this.on<RevitFamiliesListPayload>(PluginEventTypes.REVIT_FAMILIES_LIST);
  }

  /**
   * Subscribe to stamp result events
   */
  onStampResult(): Observable<RevitStampResultPayload> {
    return this.on<RevitStampResultPayload>(PluginEventTypes.REVIT_STAMP_RESULT);
  }

  /**
   * Subscribe to publish result events
   */
  onPublishResult(): Observable<RevitPublishResultPayload> {
    return this.on<RevitPublishResultPayload>(PluginEventTypes.REVIT_PUBLISH_RESULT);
  }

  /**
   * Subscribe to load result events
   */
  onLoadResult(): Observable<RevitLoadResultPayload> {
    return this.on<RevitLoadResultPayload>(PluginEventTypes.REVIT_LOAD_RESULT);
  }
}
