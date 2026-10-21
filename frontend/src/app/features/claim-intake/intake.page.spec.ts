import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { Policy } from '../../core/api/api.model';
import { apiUrl } from '../../core/api/api-url';
import { provideAppDateAdapter } from '../../core/dates/app-date-adapter';
import { NotifyService } from '../../core/notify/notify.service';
import { createPartyGroup } from '../../shared/forms/party-form';
import { createRiskObjectGroup, IntakeForm } from './intake-form';
import { IntakePage } from './intake.page';

describe('IntakePage (S:UI-Intake-Steps, S:UI-Intake-Submit)', () => {
  let http: HttpTestingController;
  const notify = { success: vi.fn(), error: vi.fn() };
  const policy: Policy = {
    id: 'a1000000-0000-4000-8000-000000000010',
    policyNumber: 'POL-48111',
    clientName: 'Harbor Logistics',
    effectiveDate: '2026-01-01',
    expirationDate: '2026-12-31',
    status: 'Active',
    coverageTypes: [],
  };

  async function render() {
    notify.success.mockClear();
    TestBed.configureTestingModule({
      imports: [IntakePage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        ...provideAppDateAdapter(),
        { provide: NotifyService, useValue: notify },
      ],
    });
    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(IntakePage);
    TestBed.tick();
    http
      .match(apiUrl('reference/cause-of-loss-codes'))
      .forEach((call) => call.flush([{ code: 'COL-WATER', name: 'Water damage', perilCategory: 'Property' }]));
    await fixture.whenStable();
    const page = fixture.componentInstance as unknown as Record<string, unknown>;

    return { fixture, page, form: page['form'] as IntakeForm };
  }

  function fill(form: IntakeForm, withRiskObject: boolean): void {
    form.controls.policy.patchValue({ selected: policy });
    form.controls.loss.patchValue({
      lossDate: new Date(Date.UTC(2026, 6, 22, 9)),
      causeOfLossCode: 'COL-WATER',
      description: 'Pipe burst under the kitchen floor',
    });
    const claimant = createPartyGroup();
    claimant.patchValue({ firstName: 'Ada' });
    form.controls.parties.push(claimant);

    if (withRiskObject) {
      const riskObject = createRiskObjectGroup();
      riskObject.patchValue({ assetDescription: 'Kitchen' });
      form.controls.riskObjects.push(riskObject);
    }
  }

  afterEach(() => http.verify());

  it('Next on an empty first step shows the errors and stays on the step', async () => {
    const { fixture, page, form } = await render();

    (page['next'] as (step: number) => void).call(fixture.componentInstance, 0);

    expect(form.controls.loss.controls.description.touched).toBe(true);
    expect((page['stepper'] as () => { selectedIndex: number })().selectedIndex).toBe(0);
  });

  it('a complete claim is created and opens its detail with the number in a snackbar', async () => {
    const { fixture, page, form } = await render();
    fill(form, true);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    (page['create'] as () => void).call(fixture.componentInstance);
    const call = http.expectOne(apiUrl('claims'));
    call.flush(
      { data: { id: 'c1', claimNumber: 'CLM-2026-0001456' }, warnings: [] },
      { status: 201, statusText: 'Created' },
    );

    expect(call.request.body.policyId).toBe(policy.id);
    expect(notify.success).toHaveBeenCalledWith('Claim CLM-2026-0001456 created.');
    expect(navigate).toHaveBeenCalledWith(['/claims', 'c1']);
  });

  it('asks for confirmation when there are warnings and sends nothing when declined', async () => {
    const { fixture, page, form } = await render();
    fill(form, false);
    const open = vi.spyOn(TestBed.inject(MatDialog), 'open').mockReturnValue({ afterClosed: () => of(false) } as never);

    (page['create'] as () => void).call(fixture.componentInstance);

    expect(open).toHaveBeenCalled();
    http.expectNone(apiUrl('claims'));
  });

  it('a 422 puts the message on its field, lists the rest and returns to its step', async () => {
    const { fixture, page, form } = await render();
    const stepper = page['stepper'] as () => { selectedIndex: number };
    fill(form, true);
    await fixture.whenStable();
    (page['goToStep'] as (step: number) => void).call(fixture.componentInstance, 2);
    const stepBefore = stepper().selectedIndex;

    (page['create'] as () => void).call(fixture.componentInstance);
    http.expectOne(apiUrl('claims')).flush(
      {
        type: 'ValidationError',
        title: 'Invalid',
        status: 422,
        errors: { 'Loss.Description': ['Too short.'], General: ['Refused.'] },
      },
      { status: 422, statusText: 'Unprocessable Entity' },
    );

    expect(form.controls.loss.controls.description.getError('server')).toBe('Too short.');
    expect((page['bannerMessages'] as () => string[])()).toEqual(['Refused.']);
    expect(stepBefore).toBe(2);
    expect(stepper().selectedIndex).toBe(0);
  });
});
