import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { LibraryService } from '../../services/library.service';
import { FamilyDetail, FamilyVersion } from '../../../../core/models/family.model';

declare global {
  interface Window {
    revitBridge?: {
      postMessage: (message: RevitBridgeMessage) => void;
    };
  }
}

interface RevitBridgeMessage {
  type: string;
  payload: unknown;
}

interface RevitFamilyLoadedEvent {
  success: boolean;
  familyName?: string;
  error?: string;
}

interface RevitFamilyLoadedDetail {
  detail: RevitFamilyLoadedEvent;
}

@Component({
  selector: 'app-family-detail',
  imports: [
    CardModule,
    TableModule,
    TagModule,
    ButtonModule,
    SkeletonModule,
    TooltipModule,
    DatePipe,
  ],
  templateUrl: './family-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FamilyDetailComponent {
  private readonly libraryService = inject(LibraryService);
  private readonly route = inject(ActivatedRoute);
  private readonly messageService = inject(MessageService);

  protected readonly family = signal<FamilyDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.loadFamily();
  }

  protected loadFamily(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Family ID not provided');
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.libraryService.getFamilyById(id).subscribe({
      next: data => {
        this.family.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load family details');
        this.loading.set(false);
      },
    });
  }

  protected downloadVersion(version: FamilyVersion): void {
    // TODO: Implement download logic via RevitBridgeService
    console.log('Download version:', version);
  }

  protected loadFamilyToProject(): void {
    const familyData = this.family();
    if (!familyData) {
      return;
    }

    const revitBridge = window.revitBridge;
    if (revitBridge) {
      revitBridge.postMessage({
        type: 'ui:load-family',
        payload: {
          familyId: familyData.id,
          version: familyData.currentVersion,
        },
      });

      this.subscribeToLoadResult();
    } else {
      const currentVersion = familyData.versions.find(v => v.version === familyData.currentVersion);
      if (currentVersion) {
        this.downloadVersion(currentVersion);
        this.messageService.add({
          severity: 'info',
          summary: 'Not in Revit',
          detail: 'Running outside Revit. Using download instead.',
        });
      }
    }
  }

  private subscribeToLoadResult(): void {
    const handler = (event: Event): void => {
      const customEvent = event as unknown as RevitFamilyLoadedDetail;
      const result = customEvent.detail;

      if (result.success) {
        this.messageService.add({
          severity: 'success',
          summary: 'Family Loaded',
          detail: `${result.familyName ?? 'Family'} loaded successfully`,
        });
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Load Failed',
          detail: result.error ?? 'Unknown error',
        });
      }

      window.removeEventListener('revit:family-loaded', handler);
    };

    window.addEventListener('revit:family-loaded', handler);
  }

  protected isCurrentVersion(version: FamilyVersion): boolean {
    const familyData = this.family();
    if (!familyData) {
      return false;
    }
    return version.version === familyData.currentVersion;
  }
}
