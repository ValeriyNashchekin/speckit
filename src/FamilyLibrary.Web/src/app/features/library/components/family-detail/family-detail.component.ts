import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TooltipModule } from 'primeng/tooltip';
import { LibraryService } from '../../services/library.service';
import { FamilyDetail, FamilyVersion } from '../../../../core/models/family.model';

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
    // TODO: Implement load to project logic via RevitBridgeService
    console.log('Load family to project:', this.family()?.id);
  }

  protected isCurrentVersion(version: FamilyVersion): boolean {
    const familyData = this.family();
    if (!familyData) {
      return false;
    }
    return version.version === familyData.currentVersion;
  }
}
