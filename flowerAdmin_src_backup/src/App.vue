<template>
  <el-config-provider :locale="locale" :size="assemblySize" :button="buttonConfig">
    <router-view></router-view>
  </el-config-provider>
</template>

<script setup lang="ts">
import { onMounted, reactive, computed } from "vue";
import { useI18n } from "vue-i18n";
import { getBrowserLang } from "@/utils";
import { useTheme } from "@/hooks/useTheme";
import { ElConfigProvider } from "element-plus";
import { LanguageType } from "./stores/interface";
import { useGlobalStore } from "@/stores/modules/global";
import en from "element-plus/es/locale/lang/en";
import zhCn from "element-plus/es/locale/lang/zh-cn";
import { sysUserApi } from "./api/api";
import { useConfigStore } from "./stores/modules/config";
import { useUserStore } from "@/stores/modules/user";
let config = useConfigStore();
const userStore = useUserStore();
if (userStore.token) {
  sysUserApi.GetSysConfig().then(res => {
    config.baseImgUrl = res.data.DomianStaticName;
  });
  userStore.updateUserInfo();
}
const globalStore = useGlobalStore();

// init theme
const { initTheme } = useTheme();
initTheme();

// init language
const i18n = useI18n();
onMounted(() => {
  const language = globalStore.language ?? getBrowserLang();
  i18n.locale.value = language;
  globalStore.setGlobalState("language", language as LanguageType);
});

// element language
const locale = computed(() => {
  if (globalStore.language == "zh") return zhCn;
  if (globalStore.language == "en") return en;
  return getBrowserLang() == "zh" ? zhCn : en;
});

// element assemblySize
const assemblySize = computed(() => globalStore.assemblySize);

// element button config
const buttonConfig = reactive({ autoInsertSpace: false });
</script>
<style>
.imgUrl {
  width: 70px;
  height: 70px;
  border-radius: 5px;
}
.imgUrl:not(:first-of-type) {
  margin-left: 10px;
}
.formFlex {
  display: flex;
}
.flexCol {
  display: flex;
  flex-direction: column;
}
.dialogScrollbar {
  max-height: 60vh;
  overflow-y: auto;
}
.dialogScrollbar::-webkit-scrollbar {
  display: none;
}
.ml20 {
  margin-left: 20px;
}
.ml12 {
  margin-left: 12px;
}
.cursorPointer {
  cursor: pointer;
}
.flexCenter {
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
