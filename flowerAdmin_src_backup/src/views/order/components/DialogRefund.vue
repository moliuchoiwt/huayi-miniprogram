<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.row?.orderNo + '退款'" width="50%" align-center>
      <el-descriptions border :column="4" label-width="100px">
        <el-descriptions-item label="订单编号">{{ FData.row.orderNo }}</el-descriptions-item>
        <el-descriptions-item label="订单总金额">￥{{ FData.row.price }}</el-descriptions-item>
      </el-descriptions>
      <div style="margin-top: 24px"></div>
      <el-form ref="ruleFormRef" label-width="110px" label-suffix=" :" :rules="rules" :model="FData?.row">
        <el-form-item label="商家退款金额" prop="shopRefundAmount">
          <el-input-number v-model="query.shopRefundAmount" :precision="2" :step="1" :min="0" />
        </el-form-item>
        <el-form-item label="用户退款金额" prop="userRefundAmount">
          <el-input-number v-model="query.userRefundAmount" :precision="2" :step="1" :min="0" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogShow = false">取消</el-button>
          <el-button type="primary" @click="handleSubmit()"> 确定 </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { ElDialog, ElMessage, FormInstance } from "element-plus";
import { sysTaskOrderApi } from "@/api/api";
const dialogShow = ref(false);
let query = ref({
  Id: 0,
  shopRefundAmount: 0,
  userRefundAmount: 0
});
const FData = ref<FProps>({
  row: {}
});
const rules = reactive({});
// 提交数据（新增/编辑）
const ruleFormRef = ref<FormInstance>();
const handleSubmit = () => {
  ruleFormRef.value!.validate(async valid => {
    if (!valid) return;
    try {
      await sysTaskOrderApi.ConfirmRefund(query.value);
      ElMessage.success({ message: `退款成功！` });
      FData.value?.getTableList!();
      dialogShow.value = false;
    } catch (error) {
      console.log(error);
    }
  });
};
interface FProps {
  row: any;
  getTableList?: () => void;
}
// 接收父组件传过来的参数
const acceptParams = async (params: FProps) => {
  FData.value = { ...params };
  query.value.Id = params.row.Id;
  dialogShow.value = true;
};

defineExpose({
  acceptParams
});
</script>
