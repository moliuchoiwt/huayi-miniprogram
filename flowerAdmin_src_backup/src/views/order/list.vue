<template>
  <div class="table-box">
    <ProTable ref="proTable" :columns="columns" row-key="Id" :request-api="sysTaskOrderApi.list">
      <template #tableHeader>
        <el-button type="success" :icon="Upload" @click="clickExportExcel">导出Excel </el-button>
      </template>
      <template #expand="scope">
        <el-descriptions border :column="2" label-width="100px">
          <el-descriptions-item label="商家名称">{{ scope.row.consignee }}</el-descriptions-item>
          <el-descriptions-item label="商家手机号">{{ scope.row.mobile }}</el-descriptions-item>
          <el-descriptions-item label="任务图片">
            <el-image
              :src="baseImgUrl(item)"
              fit="contain"
              v-for="(item, index) in scope.row.goodsImgList"
              :key="index"
              class="imgUrl"
              :preview-src-list="scope.row.goodsImgList.map(item => baseImgUrl(item))"
              :preview-index="index"
              preview-teleported
            ></el-image>
          </el-descriptions-item>
          <el-descriptions-item label="收货方式">{{ scope.row.receivingTypeName }}</el-descriptions-item>
          <el-descriptions-item label="收货时间">{{ scope.row.receivingTime }}</el-descriptions-item>
          <el-descriptions-item label="收货地址">{{
            scope.row.province + scope.row.city + scope.row.area + scope.row.address
          }}</el-descriptions-item>
          <el-descriptions-item label="相关需求">{{ scope.row.relatedDemand }}</el-descriptions-item>
          <el-descriptions-item label="备注">{{ scope.row.remarks }}</el-descriptions-item>
        </el-descriptions>
      </template>
      <template #loglist="scope">
        <div>{{ scope.row.loglist[0]?.consignee }}/{{ scope.row.loglist[0]?.mobile }}</div>
        <div>{{ scope.row.loglist[0]?.address }}</div>
      </template>
      <template #loglist2="scope">
        <div>{{ scope.row.loglist[0]?.expressName }}</div>
        <div>{{ scope.row.loglist[0]?.logisticsNo }}</div>
      </template>
      <template #time="scope">
        <div>创建时间: {{ scope.row.createTime }}</div>
        <div>修改时间: {{ scope.row.updateTime }}</div>
      </template>
      <template #operation="scope">
        <el-button type="primary" v-if="scope.row.status == 0" link @click="linePayment({ ...scope.row })"> 线下支付 </el-button>
        <el-button type="warning" v-if="scope.row.status == 1" link :icon="EditPen" @click="checkFun({ ...scope.row }, 2)">
          通过
        </el-button>
        <el-button type="danger" v-if="scope.row.status == 1" link :icon="Delete" @click="checkFun(scope.row, 9)">
          驳回
        </el-button>
        <el-button type="danger" v-if="[0, 1].includes(scope.row.status)" link :icon="Delete" @click="delFun({ ...scope.row })">
          删除
        </el-button>
        <el-button
          type="danger"
          v-if="[2, 3, 4, 5, 6].includes(scope.row.status)"
          link
          :icon="BottomLeft"
          @click="refundFun({ ...scope.row })"
        >
          退款
        </el-button>
      </template>
    </ProTable>
    <DialogRefund ref="dialogRefundRef" />
  </div>
</template>

<script setup lang="ts" name="orderList">
import { ref } from "vue";
import { ColumnProps } from "@/components/ProTable/interface";
import ProTable from "@/components/ProTable/index.vue";
import { sysTaskOrderApi } from "@/api/api";
import { useHandleData } from "@/hooks/useHandleData";
import { baseImgUrl, timeSiftFun } from "@/utils";
import { exportExcel } from "@/hooks/useDownload";
import { BottomLeft, Delete, EditPen, Upload } from "@element-plus/icons-vue";
import { statusList } from "@/utils/dict";
import DialogRefund from "@/views/order/components/DialogRefund.vue";

const proTable = ref();
const dialogRefundRef = ref<InstanceType<typeof DialogRefund> | null>(null);

// 表格配置项
const columns = ref<ColumnProps[]>([
  { prop: "queryName", label: "搜索", isShow: false, isSetting: false, search: { el: "input", key: "queryName", label: "搜索" } },
  { type: "expand", label: "Expand", width: 80, fixed: "left" },
  { prop: "Id", label: "ID", width: 100, fixed: "left" },
  { prop: "shopId", label: "商家Id", width: 100 },
  { prop: "orderNo", label: "订单号", width: 250 },
  { prop: "userId", label: "用户Id", width: 100 },
  { prop: "userName", label: "用户名称", width: 120 },
  {
    prop: "status",
    label: "状态",
    tag: true,
    search: { el: "select" },
    enum: statusList,
    width: 100
  },
  { prop: "price", label: "订单金额", width: 120 },
  { prop: "payMent", label: "支付方式", width: 100 },
  ...timeSiftFun(),
  { prop: "time", label: "操作时间", minWidth: 250 },
  { prop: "operation", label: "操作", width: 200, fixed: "right" }
]);
const linePayment = (row: any) => {
  useHandleData(sysTaskOrderApi.OfflinePayment, { Id: row.Id }, `确认线下支付订单 ${row.orderNo}`).then(() => {
    proTable.value?.getTableList();
  });
};
// 导出Excel
const clickExportExcel = () => {
  console.log(proTable.value.tableData);
  const data = JSON.parse(JSON.stringify(proTable.value.tableData));
  data.forEach(item => {
    item.status = statusList.find(status => status.value == item.status)?.label || "未知";
  });
  exportExcel(
    "订单列表",
    "ID,订单号,状态,下单用户,订单应付数,修改时间",
    data.map(item => `${item.Id},${item.orderNo},${item.status},` + `${item.userName},${item.amount},` + `${item.updateTime}`)
  );
};
const delFun = (row: any) => {
  useHandleData(sysTaskOrderApi.delete, { ids: [row.Id] }, `确认删除订单 ${row.orderNo}`).then(() => {
    proTable.value?.getTableList();
  });
};
const checkFun = (row: any, index: Number) => {
  useHandleData(sysTaskOrderApi.operation, { ...row, status: index }, `确认${index == 2 ? "通过" : "驳回"}${row.orderNo}`).then(
    () => {
      proTable.value?.getTableList();
    }
  );
};
// 退款
const refundFun = (row: any) => {
  if (row.status == 2) {
    useHandleData(sysTaskOrderApi.ConfirmRefund, { ...row }, `确认退款订单 ${row.orderNo}`).then(() => {
      proTable.value?.getTableList();
    });
    return;
  }
  dialogRefundRef.value?.acceptParams({
    row: { ...row },
    getTableList: proTable.value?.getTableList
  });
};
</script>
<style lang="scss" scoped>
.avatarImg {
  width: 60px;
  height: 60px;
  border-radius: 5px;
}
</style>
