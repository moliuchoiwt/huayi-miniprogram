<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" :request-api="sysClassApi.list" :init-param="{ queryType: 1 }">
      <!-- 表格 header 按钮 -->
      <template #tableHeader>
        <el-button type="primary" :icon="CirclePlus" @click="operate('新增')">新增类型 </el-button>
      </template>
      <template #imgUrl="scope">
        <el-image
          :src="baseImgUrl(scope.row.imgUrl)"
          fit="contain"
          class="imgUrl"
          :preview-src-list="[baseImgUrl(scope.row.imgUrl)]"
          preview-teleported
        ></el-image>
      </template>
      <template #status="scope">
        <el-switch
          v-model="scope.row!.status"
          active-text="开启"
          inactive-text="关闭"
          inline-prompt
          size="large"
          :active-value="0"
          :inactive-value="1"
          @change="statusChange({ ...scope.row })"
        />
      </template>
      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
      </template>
      <template #operation="scope">
        <el-button type="warning" link :icon="EditPen" @click="operate('编辑', { ...scope.row })"> 编辑 </el-button>
      </template>
    </ProTable>
    <Dialog ref="DialogRef"></Dialog>
  </div>
</template>

<script setup lang="ts" name="productType">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { EditPen, CirclePlus } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysClassApi } from "@/api/api";
import Dialog from "./components/Dialog.vue";
import { ElMessage } from "element-plus";
import { baseImgUrl } from "@/utils";

const proTable = ref();
const DialogRef = ref<InstanceType<typeof Dialog> | null>(null);

// 表格配置项
const columns: ColumnProps[] = [
  { prop: "queryName", label: "搜索", isShow: false, isSetting: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", width: 100, fixed: "left" },
  { prop: "title", label: "名称", width: 150 },
  { prop: "status", label: "状态", width: 150 },
  // { prop: "imgUrl", label: "图片", width: 150 },
  { prop: "time", label: "操作时间", minWidth: 250 },
  { prop: "operation", label: "操作", width: 200, fixed: "right" }
];
// 状态改变
const statusChange = (row: any) => {
  if (row.Id) {
    sysClassApi.operation(row).then(() => {
      ElMessage.success("操作成功!");
    });
  }
};
const initRow = { cType: 1, status: 0, sort: 0 };
const operate = (title: string, row: any = { ...initRow }) => {
  const params = {
    title: title + (title == "编辑" ? row.title : "") + "类型",
    row: { ...row },
    api: sysClassApi.operation,
    getTableList: proTable.value?.getTableList
  };
  DialogRef.value?.acceptParams(params);
};
</script>
<style lang="scss" scoped></style>
