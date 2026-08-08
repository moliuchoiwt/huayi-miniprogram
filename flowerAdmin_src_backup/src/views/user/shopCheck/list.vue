<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" :request-api="sysShopApi.list" default-expand-all>
      <template #tableHeader> </template>
      <template #address="scope">
        <div>{{ scope.row.province }} {{ scope.row.city }} {{ scope.row.area }} {{ scope.row.address }}</div>
      </template>
      <template #businessImg="scope">
        <el-image
          :src="baseImgUrl(scope.row.businessImg)"
          fit="cover"
          class="imgUrl"
          :preview-src-list="[baseImgUrl(scope.row.businessImg)]"
          preview-teleported
        ></el-image>
      </template>
      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
      </template>
      <template #operation="scope">
        <el-button
          type="warning"
          :disabled="scope.row.auditState == 1"
          link
          :icon="EditPen"
          @click="operate({ ...scope.row }, 1)"
        >
          通过
        </el-button>
        <el-button type="danger" :disabled="scope.row.auditState == 2" link :icon="Delete" @click="operate(scope.row, 2)">
          驳回
        </el-button>
      </template>
    </ProTable>
  </div>
</template>

<script setup lang="ts" name="bannerList">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { Delete, EditPen } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysShopApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";
import { baseImgUrl } from "@/utils";

const proTable = ref();

let isLoading = ref<boolean>(true);

// 获取数据
const getList = async () => {
  isLoading.value = false;
};
getList();
// 表格配置项
const columns = ref<ColumnProps[]>([
  { prop: "queryName", label: "搜索", isShow: false, isSetting: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", width: 80, fixed: "left" },
  { prop: "businessImg", label: "营业执照", minWidth: 120 },
  {
    prop: "auditState",
    label: "状态",
    tag: true,
    enum: [
      { label: "待审核", value: 0, tagType: "primary" },
      { label: "已通过", value: 1, tagType: "success" },
      { label: "已驳回", value: 2, tagType: "danger" }
    ],
    search: { el: "select" },
    width: 90
  },
  { prop: "name", label: "店铺名称", minWidth: 120 },
  { prop: "realName", label: "姓名", minWidth: 120 },
  { prop: "mobile", label: "手机号", minWidth: 130 },
  { prop: "address", label: "地址", minWidth: 300 },
  { prop: "time", label: "操作时间", minWidth: 250 },
  { prop: "operation", label: "操作", minWidth: 200, fixed: "right" }
]);
const operate = (row: any, index: Number) => {
  row;
  useHandleData(sysShopApi.operation, { ...row, auditState: index }, `确认${index == 1 ? "通过" : "驳回"}${row.Id}`).then(() => {
    proTable.value?.getTableList();
  });
};
</script>
<style lang="scss" scoped></style>
