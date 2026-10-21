import { guidOrEmpty, isGuid } from './api-ids';

describe('api-ids (S:API-GET-claims)', () => {
  it('isGuid accepts a GUID in any casing and nothing else', () => {
    const results = [
      isGuid('a1000000-0000-4000-8000-000000000001'),
      isGuid('A1000000-0000-4000-8000-00000000000F'),
      isGuid('abc'),
      isGuid(''),
    ];

    expect(results).toEqual([true, true, false, false]);
  });

  it('guidOrEmpty drops a value the API could not bind', () => {
    const results = [guidOrEmpty('abc'), guidOrEmpty('a1000000-0000-4000-8000-000000000001')];

    expect(results).toEqual(['', 'a1000000-0000-4000-8000-000000000001']);
  });
});
