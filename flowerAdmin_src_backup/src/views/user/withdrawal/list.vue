<template>
  <div class="table-box" v-loading="isLoading">
    <ProTable ref="proTable" :columns="columns" :request-api="sysWithdrawalApi.list">
      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
      </template>
      <template #operation="scope">
        <el-button type="warning" :disabled="scope.row.status != 0" link :icon="EditPen" @click="operate({ ...scope.row }, 1)">
          通过
        </el-button>
        <el-button type="danger" :disabled="scope.row.status != 0" link :icon="Delete" @click="operate(scope.row, 2)">
          驳回
        </el-button>
      </template>
    </ProTable>
  </div>
</template>

<script setup lang="ts" name="userList">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import { Delete, EditPen } from "@element-plus/icons-vue";
import ProTable from "@/components/ProTable/index.vue";
import { sysWithdrawalApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";

const proTable = ref();
let isLoading = ref<boolean>(false);
const columns = ref<ColumnProps[]>([
  { prop: "queryName", label: "搜索", isShow: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "编号", minWidth: 100, fixed: "left" },
  {
    prop: "status",
    label: "状态",
    tag: true,
    enum: [
      { label: "待审核", value: 0, tagType: "warning" },
      { label: "待领取", value: 1, tagType: "primary" },
      { label: "已驳回", value: 2, tagType: "danger" },
      { label: "已领取", value: 3, tagType: "success" }
    ],
    search: {
      el: "select"
    },
    minWidth: 100
  },
  { prop: "userId", label: "用户id", minWidth: 120 },
  { prop: "userName", label: "用户名称", minWidth: 120 },
  { prop: "realName", label: "真实姓名", minWidth: 120 },
  { prop: "userMobile", label: "用户电话", minWidth: 120 },
  { prop: "amount", label: "订单应付金额", minWidth: 120 },
  { prop: "time", label: "操作时间", minWidth: 250 },
  { prop: "operation", label: "操作", minWidth: 200, fixed: "right" }
]);

const operate = (row: any, index: Number) => {
  row;
  useHandleData(
    sysWithdrawalApi.AuditWithdrawal,
    { ids: [row.Id], status: index, auditInfo: "" },
    `确认${index == 1 ? "通过" : "驳回"}${row.Id}`
  ).then(() => {
    proTable.value?.getTableList();
  });
};
</script>
