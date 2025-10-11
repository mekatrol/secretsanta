<template>
  <div class="wrapper">
    <h1>Secret Santa</h1>
    <div class="giver">
      <div><label for="giver">Your Name</label></div>
      <input id="giver" type="text" v-model="giver" />
    </div>
    <div class="code">
      <label for="code">Your Code</label>
      <input id="code" type="password" v-model="code" />
    </div>
    <div class="action">
      <button @click="getReceiver">Who have I got?</button>
    </div>
    <div class="receiver" v-if="!!receiver">
      {{ receiver }}
    </div>

    <AppErrorModal />
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { http } from "./composables/http";
import AppErrorModal from "./components/AppErrorModal.vue";
import { useAppError } from "./composables/useAppError";

const giver = ref("");
const code = ref("");
const receiver = ref("");
const { notify } = useAppError();

const getReceiver = async () => {
  try {
    const r = await http.post("/SecretSanta", {
      giver: giver.value,
      code: code.value,
    });

    giver.value = r.data.giver;
    receiver.value = r.data.receiver;
  } catch (e) {
    console.log(e);
    notify(e, "Error!");
  }
};
</script>
