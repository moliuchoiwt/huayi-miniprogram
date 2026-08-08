<template>
  <div class="table-box">
    <ProTable
      ref="proTable"
      title="菜单列表"
      row-key="name"
      :indent="20"
      :columns="columns"
      :request-api="sysMenuApi.TreeList"
      :data-callback="dataCallback"
    >
      <!-- 表格 header 按钮 -->
      <template #tableHeader>
        <el-button type="primary" :icon="CirclePlus" @click="operate('新增')">新增菜单 </el-button>
      </template>
      <!-- 菜单图标 -->
      <template #icon="scope">
        <el-icon :size="18">
          <component :is="scope.row.meta.icon"></component>
        </el-icon>
      </template>
      <!-- 菜单操作 -->
      <template #operation="scope">
        <el-button type="primary" link :icon="CirclePlus" @click="operate('新增子', { ...scope.row })"> 新增子菜单 </el-button>
        <el-button type="warning" link :icon="EditPen" @click="operate('编辑', { ...scope.row })"> 编辑 </el-button>
        <el-button type="danger" link :icon="Delete" @click="operate('删除', scope.row)"> 删除 </el-button>
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
import Dialog from "./components/Dialog.vue";
import { sysMenuApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";

const proTable = ref();
const DialogRef = ref<InstanceType<typeof Dialog> | null>(null);
const dataCallback = (data: any) => {
  return {
    list: data
  };
};
// 表格配置项
const columns: ColumnProps[] = [
  { prop: "queryName", label: "搜索", isShow: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "meta.title", label: "菜单名称", align: "left", width: 200, fixed: "left" },
  { prop: "meta.icon", label: "菜单图标", width: 100 },
  { prop: "name", label: "菜单 name", width: 150 },
  { prop: "path", label: "菜单路径", width: 300 },
  { prop: "component", label: "组件路径", width: 300 },
  { prop: "operation", label: "操作", width: 300, fixed: "right" }
];
const initRow = { icon: "Menu", path: "/", component: "/", sort: 0, menuType: 0, isHide: false };
const operate = (title: string, row: any = { ...initRow }) => {
  if (title == "删除") {
    useHandleData(sysMenuApi.delete, { ids: [row.Id] }, `确认删除菜单 ${row.meta?.title} 及其子菜单`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
  const params = {
    title: title == "编辑" ? "编辑" + row.meta?.title : title == "新增子" ? "新增" + row.meta?.title + "子" : title,
    row: (() => {
      if (title == "新增子") {
        row["pid"] = row.Id;
        delete row.Id;
      }
      return row;
    })(),
    api: sysMenuApi.operation,
    getTableList: proTable.value?.getTableList
  };
  DialogRef.value?.acceptParams(params);
};
</script>
