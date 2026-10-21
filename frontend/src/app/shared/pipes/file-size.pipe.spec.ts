import { formatFileSize } from './file-size.pipe';

describe('formatFileSize (S:UI-Documents)', () => {
  it('shows bytes, kilobytes and megabytes with one decimal above bytes', () => {
    const sizes = [formatFileSize(0), formatFileSize(512), formatFileSize(245_760), formatFileSize(19_293_798)];

    expect(sizes).toEqual(['0 B', '512 B', '240.0 KB', '18.4 MB']);
  });
});
