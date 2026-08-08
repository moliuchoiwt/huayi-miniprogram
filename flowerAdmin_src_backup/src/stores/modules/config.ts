import { defineStore } from "pinia";
import piniaPersistConfig from "@/stores/helper/persist";

export const useConfigStore = defineStore({
  id: "geeker-config",
  state: () => ({
    baseImgUrl: ""
  }),
  getters: {},
  actions: {},
  persist: piniaPersistConfig("geeker-config")
});
