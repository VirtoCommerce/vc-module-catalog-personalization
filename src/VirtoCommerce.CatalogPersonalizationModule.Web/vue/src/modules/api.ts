/** Small JSON fetch (same-origin cookie auth, like the generated clients). */
export async function http<T = unknown>(method: string, url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: body !== undefined ? { "Content-Type": "application/json" } : {},
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`${method} ${url}: ${res.status} ${await res.text()}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

/** Allowed values of a global platform setting (legacy settings.getValues: needs no setting permission); [] if it fails. */
export const settingValues = (name: string) =>
  http<unknown[] | null>("GET", `/api/platform/settings/values/${encodeURIComponent(name)}`).then(
    (values) => (values ?? []).map(String),
    () => [] as string[],
  );
