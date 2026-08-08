<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" :request-api="sysWalletLogApi.list">
      <template #tableHeader>
        <el-button type="primary" :icon="CirclePlus" @click="operate('新增')"> 手动新增流水 </el-button>
      </template>
    </ProTable>
    <DialogWalletLog ref="DialogRef"></DialogWalletLog>
  </div>
</template>

<script setup lang="ts" name="bannerList">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import ProTable from "@/components/ProTable/index.vue";
import { sysWalletLogApi } from "@/api/api";
import { CirclePlus } from "@element-plus/icons-vue";
import { useHandleData } from "@/hooks/useHandleData";
import DialogWalletLog from "./components/DialogWalletLog.vue";

const proTable = ref();
const DialogRef = ref<InstanceType<typeof DialogWalletLog> | null>(null);

// 表格配置项
const columns: ColumnProps[] = [
  { prop: "queryName", label: "搜索", isShow: false, isSetting: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { prop: "Id", label: "ID", width: 100, fixed: "left" },
  { prop: "title", label: "标题", width: 150 },
  { prop: "userId", label: "用户id", width: 150 },
  { prop: "userName", label: "用户昵称", width: 150 },
  {
    prop: "sourceType",
    label: "来源",
    tag: true,
    enum: [
      { label: "订单完成", value: 2, tagType: "success" },
      { label: "订单退款", value: 3, tagType: "danger" },
      { label: "提现", value: 12, tagType: "warning" }
    ],
    search: {
      el: "select"
    },
    minWidth: 120
  },
  { prop: "orderNo", label: "关联单号", minWidth: 230 },
  { prop: "remark", label: "备注", minWidth: 300 },
  { prop: "change", label: "资金变化", minWidth: 100, fixed: "right" },
  { prop: "createTime", label: "操作时间", minWidth: 200, fixed: "right" }
];
const initRow = {};
const operate = (title: string, row: any = { ...initRow }) => {
  if (title == "删除") {
    useHandleData(sysWalletLogApi.delete, { ids: [row.Id] }, `确认删除流水 ${row.title}`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
  const params = {
    title: title + (title == "编辑" ? row.title : "") + "流水",
    row: { ...row },
    api: sysWalletLogApi.operation,
    getTableList: proTable.value?.getTableList
  };
  DialogRef.value?.acceptParams(params);
};
</script>
<style lang="scss" scoped></style>
