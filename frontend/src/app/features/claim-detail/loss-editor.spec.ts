import { TestBed } from '@angular/core/testing';
import { LossRequest } from '../../core/api/api.model';
import { provideAppDateAdapter } from '../../core/dates/app-date-adapter';
import { claimDetail } from './claim-detail.fixtures';
import { LossEditor } from './loss-editor';

describe('LossEditor (S:API-PUT-claims-loss)', () => {
  const causes = [{ code: 'COL-WATER', name: 'Water damage', perilCategory: 'Property' }];

  async function render(loss = claimDetail().loss) {
    TestBed.configureTestingModule({ providers: [...provideAppDateAdapter()] });
    const fixture = TestBed.createComponent(LossEditor);
    fixture.componentRef.setInput('loss', loss);
    fixture.componentRef.setInput('causeCodes', causes);
    await fixture.whenStable();
    const submitted: LossRequest[] = [];
    fixture.componentInstance.submitted.subscribe((request) => submitted.push(request));
    const element = fixture.nativeElement as HTMLElement;
    const save = () =>
      [...element.querySelectorAll('button')].find((button) => button.textContent?.trim() === 'Save loss details');

    return { fixture, element, submitted, save };
  }

  it('saves the full loss, keeping the fields that were not changed', async () => {
    const { fixture, element, submitted, save } = await render();
    const police = element.querySelector<HTMLInputElement>('input[formcontrolname="policeReportNumber"]');
    police!.value = ' PR-9 ';
    police!.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    save()?.click();

    const loss = claimDetail().loss;
    expect(submitted).toEqual([
      {
        lossDate: new Date(loss.lossDate).toISOString(),
        description: loss.description,
        causeOfLossCode: loss.causeOfLossCode,
        location: loss.location,
        estimatedLossAmount: loss.hasEstimatedLossAmount ? loss.estimatedLossAmount : null,
        policeReportNumber: 'PR-9',
      },
    ]);
  });

  it('does not save a description below the minimum', async () => {
    const { fixture, element, submitted, save } = await render();
    const description = element.querySelector<HTMLTextAreaElement>('textarea[formcontrolname="description"]');
    description!.value = 'Too short';
    description!.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    save()?.click();

    expect(submitted).toEqual([]);
  });
});
