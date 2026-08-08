<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.title" width="50%" align-center>
      <el-form ref="ruleFormRef" label-width="100px" label-suffix=" :" :rules="rules" :model="FData?.row">
        <el-form-item label="图片" prop="imgUrl">
          <UploadImg v-model:image-url="FData.row!.imgUrl" width="135px" height="135px" :file-size="3" />
        </el-form-item>
        <el-form-item label="名称" prop="title">
          <el-input v-model="FData.row!.title" placeholder="请填写名称" clearable></el-input>
        </el-form-item>
        <el-form-item label="跳转链接" prop="link">
          <el-input v-model="FData.row!.link" placeholder="请填写跳转链接" clearable></el-input>
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
const dialogShow = ref(false);
const FData = ref<FProps>({
  title: "",
  row: {}
});
const rules = reactive({
  imgUrl: [{ required: true, message: "请上传图片" }],
  title: [{ required: true, message: "请填写名称" }]
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
