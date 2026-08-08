<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" :request-api="sysRoleApi.list" :data-callback="dataCallback">
      <!-- 表格 header 按钮 -->
      <template #tableHeader>
        <el-button type="primary" :icon="CirclePlus" @click="operate('新增')">新增角色 </el-button>
      </template>
      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
      </template>
      <!-- 角色操作 -->
      <template #operation="scope">
        <el-button type="warning" link :icon="EditPen" @click="operate('编辑', { ...scope.row })"> 编辑 </el-button>
        <el-button type="danger" :disabled="scope.row.Id <= 2" link :icon="Delete" @click="operate('删除', scope.row)">
          删除
        </el-button>
      </template>
    </ProTable>
    <Dialog ref="DialogRef"></Dialog>
  </div>
</template>

<script setup lang="ts" name="menuMange">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { Delete, EditPen, CirclePlus } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysRoleApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";
import Dialog from "./components/Dialog.vue";

const proTable = ref();
const DialogRef = ref<InstanceType<typeof Dialog> | null>(null);
const dataCallback = (data: any) => {
  return {
    list: data.items,
    total: data.total
  };
};
// 表格配置项
const columns: ColumnProps[] = [
  { prop: "queryName", label: "搜索", isShow: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", width: 100, fixed: "left" },
  { prop: "roleName", label: "名称", width: 150 },
  { prop: "remark", label: "备注" },
  { prop: "time", label: "操作时间" },
  { prop: "operation", label: "操作", width: 300, fixed: "right" }
];
const initRow = {};
const operate = (title: string, row: any = { ...initRow }) => {
  if (title == "删除") {
    useHandleData(sysRoleApi.delete, { ids: [row.Id] }, `确认删除角色 ${row.roleName} 及其子角色`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
  const params = {
    title: title + (title == "编辑" ? row.roleName : ""),
    row: { ...row },
    api: sysRoleApi.operation,
    getTableList: proTable.value?.getTableList
  };
  DialogRef.value?.acceptParams(params);
};
</script>
