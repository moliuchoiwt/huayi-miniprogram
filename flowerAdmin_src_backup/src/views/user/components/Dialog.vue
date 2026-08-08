<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.title + '用户'" width="50%" align-center>
      <el-form ref="ruleFormRef" label-width="110px" label-suffix=" :" :rules="rules" :model="FData?.row">
        <el-form-item label="用户头像" prop="avatar">
          <UploadImg v-model:image-url="FData.row!.avatar" width="135px" height="135px" :file-size="3">
            <template #empty>
              <el-icon><Avatar /></el-icon>
              <span>请上传头像</span>
            </template>
            <template #tip> 头像大小不能超过 3M </template>
          </UploadImg>
        </el-form-item>
        <el-form-item label="名称" prop="nickName">
          <el-input v-model="FData.row!.nickName" placeholder="请填写名称" clearable></el-input>
        </el-form-item>
        <el-form-item label="手机号" prop="mobile">
          <el-input v-model="FData.row!.mobile" disabled type="number" placeholder="请填写手机号" clearable></el-input>
        </el-form-item>
        <el-form-item label="上级用户" prop="parentId">
          <el-select v-model="FData.row!.parentId" filterable remote placeholder="请选择上级用户" :remote-method="getSysUserList">
            <el-option v-for="item in userList" :key="item.Id" :label="item.Id + '-' + item.nickName" :value="item.Id" />
          </el-select>
        </el-form-item>
        <el-form-item label="会员等级" prop="gradeId">
          <el-select v-model="FData.row!.gradeId" filterable remote placeholder="请选择会员等级">
            <el-option v-for="item in userGradeList" :key="item.Id" :label="item.name" :value="item.Id" />
          </el-select>
        </el-form-item>
        <el-form-item label="会员过期时间" prop="gradeId" v-if="FData.row!.gradeId">
          <el-date-picker
            v-model="FData.row!.gradeExpirationDate"
            type="date"
            format="YYYY/MM/DD"
            value-format="YYYY/MM/DD"
            placeholder="请选择时间"
            :disabled-date="disabledDate"
            :clearable="false"
          />
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
import { checkPhoneNumber } from "@/utils/eleValidate";
import { sysUserGradeApi, sysUserInfoApi } from "@/api/api";
const dialogShow = ref(false);

const FData = ref<FProps>({
  title: "",
  row: {}
});
const rules = reactive({
  avatar: [{ required: true, message: "请上传用户头像" }],
  nickName: [{ required: true, message: "请填写名称" }],
  mobile: [{ required: true, message: "请填写手机号", validator: checkPhoneNumber }]
});
let userList = ref<any[]>([]);
// 获取用户列表
const getSysUserList = async (queryName: string = "") => {
  let res: any = await sysUserInfoApi.list({ pageNum: 1, pageSize: 30, queryName, status: 0 });
  userList.value = res.data.list;
};
getSysUserList();

let userGradeList = ref<any[]>([{ Id: 0, name: "普通用户" }]);
// 获取用户等级列表
const getSysUserGradeList = async () => {
  let res: any = await sysUserGradeApi.list({ pageNum: 1, pageSize: 10 });
  userGradeList.value.push(...res.data.list);
};
getSysUserGradeList();
const disabledDate = (time: Date) => {
  return time.getTime() < Date.now();
};
// 是否在今天及之前
const isOnOrBeforeToday = (dateTimeStr: string) => {
  // 解析输入字符串
  const [datePart, timePart] = dateTimeStr.split(" ");
  const [year, month, day] = datePart.split("/").map(Number);
  const [hours, minutes, seconds] = timePart.split(":").map(Number);

  // 创建输入日期对象（注意月份是0-based）
  const inputDate = new Date(year, month - 1, day, hours, minutes, seconds);

  // 获取今天的日期（时间部分设为23:59:59.999以确保包含整天）
  const today = new Date();
  today.setHours(23, 59, 59, 999);

  // 比较日期
  return inputDate <= today;
};
// 提交数据（新增/编辑）
const ruleFormRef = ref<FormInstance>();
const handleSubmit = () => {
  ruleFormRef.value!.validate(async valid => {
    if (!valid) return;
    try {
      let data = {
        ...FData.value?.row
      };
      if (!data.gradeExpirationDate.length && isOnOrBeforeToday(data.gradeExpirationDate)) {
        ElMessage.error("请选择会员过期时间!");
        return;
      }
      if (data.gradeExpirationDate.length < 13) {
        data.gradeExpirationDate += " 00:00:00";
      }
      await FData.value?.api!(data);
      ElMessage.success({ message: `${FData.value?.title}用户成功！` });
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
