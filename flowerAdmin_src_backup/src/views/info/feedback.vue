<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" :request-api="sysFeedbackApi.list">
      <!-- 用户操作 -->
      <template #avatar="scope">
        <el-image :src="scope.row.avatar" class="avatarImg"></el-image>
      </template>
      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
      </template>
      <template #operation="scope">
        <el-button type="danger" link :icon="Delete" @click="operate('删除', scope.row)"> 删除 </el-button>
      </template>
    </ProTable>
  </div>
</template>

<script setup lang="ts" name="userList">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { Delete } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysFeedbackApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";

const proTable = ref();

// 表格配置项
const columns: ColumnProps[] = [
  { prop: "queryName", label: "搜索", isShow: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", width: 100, fixed: "left" },
  { prop: "title", label: "标题", width: 150 },
  { prop: "contact", label: "手机号", width: 150 },
  { prop: "contents", label: "反馈内容", width: 250 },
  { prop: "time", label: "操作时间", width: 250 },
  { prop: "operation", label: "操作", width: 200, fixed: "right" }
];
const initRow = { status: 0 };
const operate = (title: string, row: any = { ...initRow }) => {
  if (title == "删除") {
    useHandleData(sysFeedbackApi.delete, { ids: [row.Id] }, `确认删除 ${row.title}`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
};
</script>
<style lang="scss" scoped></style>
