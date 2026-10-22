import { FormControl, FormGroup, Validators } from '@angular/forms';
import { isSetDate, NO_DATE, parseIsoDateTime, toIsoDateTime } from '../../core/api/api-dates';
import { FIELD_LENGTHS, MINIMUM_DESCRIPTION_LENGTH } from '../../core/api/api-limits';
import { LossDetails, LossRequest } from '../../core/api/api.model';
import { minTrimmedLength, notInFuture, oneOf } from '../validators/claim-validators';

export function createLossGroup(causeCodes: () => readonly string[], now: () => Date = () => new Date()) {
  return new FormGroup({
    lossDate: new FormControl<Date | null>(null, [Validators.required, notInFuture(now)]),
    causeOfLossCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, oneOf(causeCodes)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, minTrimmedLength(MINIMUM_DESCRIPTION_LENGTH)],
    }),
    location: new FormControl('', { nonNullable: true, validators: Validators.maxLength(FIELD_LENGTHS.location) }),
    estimatedLossAmount: new FormControl<number | null>(null, Validators.min(0)),
    policeReportNumber: new FormControl('', {
      nonNullable: true,
      validators: Validators.maxLength(FIELD_LENGTHS.policeReportNumber),
    }),
  });
}

export type LossGroup = ReturnType<typeof createLossGroup>;
export type LossValue = ReturnType<LossGroup['getRawValue']>;

export function toLossRequest(value: LossValue): LossRequest {
  return {
    lossDate: value.lossDate ? toIsoDateTime(value.lossDate) : NO_DATE,
    description: value.description.trim(),
    causeOfLossCode: value.causeOfLossCode,
    location: value.location.trim(),
    estimatedLossAmount: value.estimatedLossAmount,
    policeReportNumber: value.policeReportNumber.trim(),
  };
}

export function lossValueOf(loss: LossDetails): LossValue {
  return {
    lossDate: isSetDate(loss.lossDate) ? parseIsoDateTime(loss.lossDate) : null,
    causeOfLossCode: loss.causeOfLossCode,
    description: loss.description,
    location: loss.location,
    estimatedLossAmount: loss.hasEstimatedLossAmount ? loss.estimatedLossAmount : null,
    policeReportNumber: loss.policeReportNumber,
  };
}
