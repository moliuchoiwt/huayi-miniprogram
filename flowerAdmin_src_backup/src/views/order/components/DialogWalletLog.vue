<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.title" width="50%" align-center>
      <el-form ref="ruleFormRef" label-width="100px" label-suffix=" :" :rules="rules" :model="FData?.row">
        <el-form-item label="标题" prop="title" required>
          <el-input v-model="FData.row!.title" placeholder="请填写标题" clearable></el-input>
        </el-form-item>
        <el-form-item label="用户" prop="userId" required>
          <el-select v-model="FData.row!.userId" filterable remote placeholder="请选择用户" :remote-method="getSysUserList">
            <el-option v-for="item in userList" :key="item.Id" :label="item.Id + '-' + item.nickName" :value="item.Id" />
          </el-select>
        </el-form-item>
        <el-form-item label="来源" prop="sourceType" required>
          <el-select v-model="FData.row!.sourceType" filterable remote placeholder="请选择来源" :remote-method="getSysUserList">
            <el-option v-for="item in sourceTypeList" :key="item.value" :label="item.label" :value="item.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="关联单号" prop="orderNo" required>
          <el-input v-model="FData.row!.orderNo" placeholder="请填写关联单号" clearable></el-input>
        </el-form-item>
        <el-form-item label="备注" prop="remark" required>
          <el-input v-model="FData.row!.remark" type="textarea" placeholder="请填写备注" clearable></el-input>
        </el-form-item>
        <el-form-item label="资金变化" prop="change" required>
          <el-input v-model="FData.row!.change" placeholder="请填写资金变化" clearable></el-input>
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
import { sysUserInfoApi } from "@/api/api";
const dialogShow = ref(false);
const FData = ref<FProps>({
  title: "",
  row: {}
});
const rules = reactive({
  title: [{ required: true, message: "请填写名称" }]
});
let sourceTypeList = ref<any[]>([
  { label: "订单完成", value: 2, tagType: "success" },
  { label: "订单退款", value: 3, tagType: "danger" },
  { label: "提现", value: 12, tagType: "warning" }
]);
let userList = ref<any[]>([]);
// 获取用户列表
const getSysUserList = async (queryName: string = "") => {
  let res: any = await sysUserInfoApi.list({ pageNum: 1, pageSize: 30, queryName, status: 0 });
  userList.value = res.data.list;
};
getSysUserList();
// 提交数据（新增/编辑）
const ruleFormRef = ref<FormInstance>();
const handleSubmit = () => {
  ruleFormRef.value!.validate(async valid => {
    if (!valid) return;
    try {
      let data = {
        ...FData.value?.row
      };
      await FData.value?.api!(data);
      ElMessage.success({ message: `${FData.value?.title}成功！` });
      FData.value?.getTableList!();
      dialogShow.value = false;
    } catch (error) {
      console.log(error);
    }
  });
};
interface FProps {
  title: string;
  row: any;
  api?: (params: any) => Promise<any>;
  getTableList?: () => void;
}
// 接收父组件传过来的参数
const acceptParams = async (params: FProps) => {
  FData.value = { ...params };
  dialogShow.value = true;
};

defineExpose({
  acceptParams
});
</script>

<style lang="scss" scoped>
.formFlex {
  display: flex;
  justify-content: space-around;
}
</style>
