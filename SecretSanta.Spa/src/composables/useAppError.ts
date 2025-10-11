import { ref } from "vue";

export type AppError = {
  id: number;
  message: string;
  status?: number;
  title?: string;
};

const list = ref<AppError[]>([]);
let nextId = 1;

function toMessage(err: unknown, fallback = "Unexpected error") {
  // Axios-style unwrap
  const any = err as any;
  const status = any?.response?.status as number | undefined;
  const ct = (
    any?.response?.headers?.["content-type"] as string | undefined
  )?.toLowerCase();

  // 1) If the server returned JSON (ProblemDetails, custom DTO, or model-state)
  const data = any?.response?.data;
  if (data && (ct?.includes("application/json") || typeof data === "object")) {
    // RFC7807 ProblemDetails
    if (typeof data?.detail === "string") return data.detail;
    if (typeof data?.title === "string") return data.title;
    // Common API shape { message: "..."}
    if (typeof data?.message === "string") return data.message;
    // ModelState: { errors: { field: [..] } }
    if (data?.errors && typeof data.errors === "object") {
      const firstKey = Object.keys(data.errors)[0];
      const firstMsg = Array.isArray(data.errors[firstKey])
        ? data.errors[firstKey][0]
        : undefined;
      if (typeof firstMsg === "string") return firstMsg;
    }
  }

  // 2) ASP.NET Dev exception as text/plain with stacktrace
  if (typeof data === "string") {
    // Take the first line
    const firstLine = data.split(/\r?\n/)[0] ?? "";
    // Prefer the part after the first ":" if present
    const afterColon = firstLine.split(":").slice(1).join(":").trim();
    if (afterColon) return afterColon; // e.g. "Invalid name or code"

    // Fallback regex for known types
    const m =
      firstLine.match(/Exception:\s*(.+)$/i) || firstLine.match(/:\s*(.+)$/);
    if (m?.[1]) return m[1].trim();

    // If first line is empty, try the second
    const second = data.split(/\r?\n/)[1]?.trim();
    if (second) return second;
  }

  // 3) Network or generic error
  if (typeof any?.message === "string") return any.message;

  // 4) Status-based fallback
  if (status === 400) return "Bad request";
  if (status === 401) return "Unauthorized";
  if (status === 403) return "Forbidden";
  if (status === 404) return "Not found";
  if (status && status >= 500) return "Server error";

  return fallback;
}

export function useAppError() {
  function notify(err: unknown, title?: string) {
    const status = (err as any)?.response?.status as number | undefined;
    list.value.push({
      id: nextId++,
      message: toMessage(err),
      status,
      title,
    });
  }
  function remove(id: number) {
    const i = list.value.findIndex((e) => e.id === id);
    if (i >= 0) list.value.splice(i, 1);
  }
  function clear() {
    list.value = [];
  }
  return { list, notify, remove, clear };
}
