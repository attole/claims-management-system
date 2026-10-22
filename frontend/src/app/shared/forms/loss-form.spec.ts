import { LossDetails } from '../../core/api/api.model';
import { createLossGroup, lossValueOf, toLossRequest } from './loss-form';

describe('loss form (S:API-PUT-claims-loss)', () => {
  const now = () => new Date(2026, 9, 21, 12, 0);
  const saved: LossDetails = {
    id: 'l1',
    claimId: 'c1',
    reportDate: '2026-07-23T08:15:00Z',
    lossDate: '2026-07-22T09:40:00Z',
    description: 'Pipe burst under the kitchen floor',
    causeOfLossCode: 'COL-WATER',
    location: 'Dockside',
    hasEstimatedLossAmount: false,
    estimatedLossAmount: 0,
    policeReportNumber: 'PR-77',
  };

  it('fills from saved details and maps back to the same request', () => {
    const group = createLossGroup(() => ['COL-WATER'], now);

    group.setValue(lossValueOf(saved));

    expect(group.valid).toBe(true);
    expect(toLossRequest(group.getRawValue())).toEqual({
      lossDate: '2026-07-22T09:40:00.000Z',
      description: 'Pipe burst under the kitchen floor',
      causeOfLossCode: 'COL-WATER',
      location: 'Dockside',
      estimatedLossAmount: null,
      policeReportNumber: 'PR-77',
    });
  });

  it('keeps a saved estimated amount and sends trimmed text', () => {
    const group = createLossGroup(() => ['COL-WATER'], now);
    group.setValue(lossValueOf({ ...saved, hasEstimatedLossAmount: true, estimatedLossAmount: 62500 }));

    group.patchValue({ location: ' Unit 3 ', policeReportNumber: ' PR-78 ' });

    expect(toLossRequest(group.getRawValue())).toEqual(
      expect.objectContaining({ estimatedLossAmount: 62500, location: 'Unit 3', policeReportNumber: 'PR-78' }),
    );
  });

  it('flags a cause code that is no longer active', () => {
    const group = createLossGroup(() => ['COL-FIRE'], now);

    group.setValue(lossValueOf(saved));

    expect(group.controls.causeOfLossCode.invalid).toBe(true);
  });
});
