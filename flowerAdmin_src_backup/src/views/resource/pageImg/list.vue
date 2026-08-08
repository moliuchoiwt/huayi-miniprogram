<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" :request-api="sysBannerApi.list" :init-param="{ queryType: 1 }">
      <!-- 表格 header 按钮 -->
      <template #tableHeader>
        <el-button type="primary" v-if="isDevelopment" :icon="CirclePlus" @click="operate('新增')">新增图片 </el-button>
      </template>
      <!-- 图片操作 -->
      <template #imgUrl="scope">
        <el-image
          :src="baseImgUrl(scope.row.imgUrl)"
          fit="contain"
          class="imgUrl"
          :preview-src-list="[baseImgUrl(scope.row.imgUrl)]"
          preview-teleported
        ></el-image>
      </template>

      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
      </template>
      <template #operation="scope">
        <el-button type="warning" link :icon="EditPen" @click="operate('编辑', { ...scope.row })"> 编辑 </el-button>
        <!-- <el-button type="danger" link :icon="Delete" @click="operate('删除', scope.row)"> 删除 </el-button> -->
      </template>
    </ProTable>
    <Dialog ref="DialogRef"></Dialog>
  </div>
</template>

<script setup lang="ts" name="bannerList">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { EditPen, CirclePlus } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysBannerApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";
import Dialog from "./components/Dialog.vue";
import { baseImgUrl } from "@/utils";
const isDevelopment = import.meta.env.VITE_USER_NODE_ENV == "development";
const proTable = ref();
const DialogRef = ref<InstanceType<typeof Dialog> | null>(null);

// 表格配置项
const columns: ColumnProps[] = [
  { prop: "queryName", label: "搜索", isShow: false, isSetting: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", width: 100, fixed: "left" },
  { prop: "title", label: "名称", width: 150 },
  { prop: "imgUrl", label: "图片", width: 150 },
  { prop: "link", label: "跳转链接", minWidth: 250 },
  { prop: "time", label: "操作时间", minWidth: 250 },
  { prop: "operation", label: "操作", width: 200, fixed: "right" }
];
const initRow = { bType: 1 };
const operate = (title: string, row: any = { ...initRow }) => {
  if (title == "删除") {
    useHandleData(sysBannerApi.delete, { ids: [row.Id] }, `确认删除 ${row.title}`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
  const params = {
    title: title + (title == "编辑" ? row.title : "") + "图片",
    row: { ...row },
    api: sysBannerApi.operation,
    getTableList: proTable.value?.getTableList
  };
  DialogRef.value?.acceptParams(params);
};
</script>
<style lang="scss" scoped></style>
