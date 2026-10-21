export const FIELD_LENGTHS = {
  causeOfLossCode: 50,
  location: 500,
  policeReportNumber: 100,
  firstName: 100,
  lastName: 100,
  companyName: 255,
  email: 255,
  phone: 50,
  assetDescription: 500,
  assetReference: 255,
  changeReason: 500,
  reasonCode: 50,
  reasonText: 500,
  rejectionReason: 500,
  documentType: 100,
  documentNotes: 500,
} as const;

export const MINIMUM_DESCRIPTION_LENGTH = 20;

export const MAX_UPLOAD_BYTES = 50 * 1024 * 1024;
export const ALLOWED_UPLOAD_TYPES: readonly string[] = [
  'application/pdf',
  'image/jpeg',
  'image/png',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  'text/plain',
  'text/csv',
];
