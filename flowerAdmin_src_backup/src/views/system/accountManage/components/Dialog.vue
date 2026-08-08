<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.title + '账号'" width="50%" align-center>
      <el-form ref="ruleFormRef" label-width="100px" label-suffix=" :" :rules="rules" :model="FData?.row">
        <el-form-item label="用户头像" prop="avatar">
          <UploadImg v-model:image-url="FData.row!.avatar" width="135px" height="135px" :file-size="3">
            <template #empty>
              <el-icon><Avatar /></el-icon>
              <span>请上传头像</span>
            </template>
            <template #tip> 头像大小不能超过 3M </template>
          </UploadImg>
        </el-form-item>
        <el-form-item label="权限" prop="roleId" v-if="FData.row!.Id != 1">
          <el-select v-model="FData.row!.roleId" placeholder="请选择权限" style="width: 240px">
            <el-option v-for="item in roleList" :key="item.Id" :label="item.roleName" :value="item.Id" />
          </el-select>
        </el-form-item>
        <el-form-item label="名称" prop="nickName">
          <el-input v-model="FData.row!.nickName" placeholder="请填写名称" clearable></el-input>
        </el-form-item>
        <el-form-item label="账号" prop="userName">
          <el-input v-model="FData.row!.userName" placeholder="请填写账号" clearable></el-input>
        </el-form-item>
        <el-form-item label="手机号" prop="phone">
          <el-input v-model="FData.row!.phone" type="number" placeholder="请填写手机号" clearable></el-input>
        </el-form-item>
        <el-form-item label="密码" prop="pwd">
          <el-input v-model="FData.row!.pwd" type="password" placeholder="请填写密码" clearable></el-input>
        </el-form-item>
        <el-form-item label="状态">
          <el-radio-group v-model="FData.row!.accountStatus">
            <el-radio-button :label="item" :value="index" v-for="(item, index) in ['开启', '关闭']" :key="index" />
          </el-radio-group>
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
import UploadImg from "@/components/Upload/Img.vue";
import { ElDialog, ElMessage, FormInstance } from "element-plus";

let roleList = ref<any[]>([]);
const dialogShow = ref(false);
const FData = ref<FProps>({
  title: "",
  row: {},
  other: []
});

const rules = reactive({
  avatar: [{ required: true, message: "请上传用户头像" }],
  nickName: [{ required: true, message: "请填写名称" }],
  userName: [{ required: true, message: "请填写账号" }],
  // phone: [{ required: true, message: "请填写手机号", validator: checkPhoneNumber }],
  pwd: [{ required: true, message: "请填写密码" }],
  roleId: [{ required: true, message: "请选择权限" }]
});
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
      ElMessage.success({ message: `${FData.value?.title}账号成功！` });
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
  other: any[];
}
// 接收父组件传过来的参数
const acceptParams = async (params: FProps) => {
  FData.value = { ...params };
  dialogShow.value = true;
  roleList.value = FData.value.other[0];
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
