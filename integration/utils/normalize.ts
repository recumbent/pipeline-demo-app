export function normaliseStringForFilename(str: string): string {
  return (
    str
      // remove quotes
      .replace(/['"]+/g, "")
      // anything that isn't a number or string replace with a dash
      .replace(/[^a-z0-9]/gi, "-")
  );
}
