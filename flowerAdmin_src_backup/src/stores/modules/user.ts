import { defineStore } from "pinia";
import { UserState } from "@/stores/interface";
import piniaPersistConfig from "@/stores/helper/persist";
import { sysUserApi } from "@/api/api";

export const useUserStore = defineStore({
  id: "geeker-user",
  state: (): UserState => ({
    token: "",
    userInfo: { name: "Geeker" }
  }),
  getters: {},
  actions: {
    // Set Token
    setToken(token: string) {
      this.token = token;
    },
    async updateUserInfo() {
      const { data } = await sysUserApi.AdminInfo();
      this.userInfo = data;
    }
  },
  persist: piniaPersistConfig("geeker-user")
});
