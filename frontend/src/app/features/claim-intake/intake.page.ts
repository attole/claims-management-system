import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { Router, RouterLink } from '@angular/router';
import { filter, of, switchMap } from 'rxjs';
import { Claim, WriteResult } from '../../core/api/api.model';
import { readErrorBody } from '../../core/http/error-body';
import { NotifyService } from '../../core/notify/notify.service';
import { confirmWith } from '../../shared/ui/confirm-dialog';
import { applyServerErrors } from '../../shared/validators/server-errors';
import { createIntakeForm, intakeWarnings, toCreateRequest } from './intake-form';
import { IntakeStore } from './intake.store';
import { PartiesStep } from './steps/parties-step';
import { PolicyLossStep } from './steps/policy-loss-step';
import { ReserveReviewStep } from './steps/reserve-review-step';

@Component({
  selector: 'app-intake-page',
  imports: [RouterLink, MatButtonModule, MatStepperModule, PolicyLossStep, PartiesStep, ReserveReviewStep],
  templateUrl: './intake.page.html',
  styleUrl: './intake.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [IntakeStore],
})
export class IntakePage {
  protected readonly store = inject(IntakeStore);
  private readonly dialog = inject(MatDialog);
  private readonly notify = inject(NotifyService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly stepper = viewChild.required(MatStepper);

  protected readonly form = createIntakeForm(() => this.store.causeCodes().map((cause) => cause.code));
  protected readonly bannerMessages = signal<string[]>([]);

  private readonly formEvents = toSignal(this.form.events);

  protected readonly formValue = computed(() => {
    this.formEvents();

    return this.form.getRawValue();
  });

  protected readonly isStep1Valid = computed(() => {
    this.formEvents();

    return this.form.controls.policy.valid && this.form.controls.loss.valid;
  });

  protected readonly isStep2Valid = computed(() => {
    this.formEvents();

    return this.form.controls.parties.valid && this.form.controls.riskObjects.valid;
  });

  protected readonly canCreate = computed(() => {
    this.formEvents();

    return this.form.valid && !this.store.isSubmitting();
  });

  protected next(step: number): void {
    const groups = this.groupsOf(step);
    groups.forEach((group) => group.markAllAsTouched());

    if (groups.every((group) => group.valid)) {
      this.stepper().next();
    }
  }

  protected goToStep(step: number): void {
    this.stepper().selectedIndex = step;
  }

  protected create(): void {
    this.form.markAllAsTouched();

    if (!this.form.valid) {
      return;
    }

    const value = this.form.getRawValue();
    const warnings = intakeWarnings(value);
    const confirmed = warnings.length === 0 ? of(true) : confirmWith(this.dialog, confirmation(warnings));

    confirmed
      .pipe(
        filter((isConfirmed) => isConfirmed),
        switchMap(() => this.store.create(toCreateRequest(value, new Date()))),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (result) => this.created(result),
        error: (error: unknown) => this.refused(error),
      });
  }

  private created(result: WriteResult<Claim>): void {
    const notes = result.warnings.length > 0 ? ` with ${result.warnings.length} warning(s)` : '';
    this.notify.success(`Claim ${result.data.claimNumber} created${notes}.`);
    void this.router.navigate(['/claims', result.data.id]);
  }

  private refused(error: unknown): void {
    const isFormError = error instanceof HttpErrorResponse && error.status === HttpStatusCode.UnprocessableEntity;

    if (!isFormError) {
      return;
    }

    this.bannerMessages.set(applyServerErrors(this.form, readErrorBody(error)));
    const firstInvalid = [0, 1, 2].find((step) => this.groupsOf(step).some((group) => group.invalid)) ?? 2;
    this.goToStep(firstInvalid);
  }

  private groupsOf(step: number) {
    const controls = this.form.controls;
    const steps = [[controls.policy, controls.loss], [controls.parties, controls.riskObjects], [controls.reserve]];

    return steps[step] ?? [];
  }
}

function confirmation(warnings: string[]) {
  return {
    title: 'Create the claim with warnings?',
    message: 'The claim can be created, but check these points first:',
    details: warnings,
    confirmLabel: 'Create anyway',
  };
}
