<template>
  <div
    v-if="current"
    class="overlay"
    @click.self="dismiss"
    role="dialog"
    aria-modal="true"
    aria-labelledby="err-title"
  >
    <div class="modal">
      <div class="header">
        <div class="title">
          <strong id="err-title">{{ current.title ?? "Error" }}</strong>
          <span v-if="current.status" class="code">{{ current.status }}</span>
        </div>
      </div>
      <div class="body">{{ current.message }}</div>
      <button class="close" @click="dismiss" aria-label="Close">Close</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onBeforeUnmount } from "vue";
import { useAppError } from "../composables/useAppError";

const { list, remove } = useAppError();
const current = computed(() => list.value[0] ?? null);

function dismiss() {
  if (current.value) remove(current.value.id);
}

function onKey(e: KeyboardEvent) {
  if (e.key === "Escape") dismiss();
}

onMounted(() => window.addEventListener("keydown", onKey));
onBeforeUnmount(() => window.removeEventListener("keydown", onKey));
</script>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  display: grid;
  place-items: center;
  background: rgba(0, 0, 0, 0.45);
  z-index: 10000;
}

.modal {
  width: min(90vw, 520px);
  background: #fff;
  color: #111;
  border-radius: 12px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.25);
  padding: 14px 16px 16px;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between; /* pushes button to the right */
  gap: 0.5rem;
}

.code {
  margin-left: 0.25rem;
  font-size: 0.85rem;
  padding: 0 0.35rem;
  border-radius: 0.25rem;
  border: 1px solid #f0caca;
  background: #fff5f5;
  color: #a00;
}

.title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.close {
  margin-top: 2rem;
  background: #a00;
  color: #fff5f5;
  appearance: none;
  border: none;
  font-size: 1rem;
  line-height: 1;
  cursor: pointer;
}

.body {
  margin-top: 0.5rem;
  white-space: pre-wrap;
  word-break: break-word;
}
</style>
