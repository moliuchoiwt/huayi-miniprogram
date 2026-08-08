<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.title" width="80%" align-center>
      <el-tabs type="border-card">
        <el-tab-pane label="基础信息">
          <div class="dialogScrollbar">
            <el-form ref="ruleFormRef" label-width="80px" label-suffix=" :" :rules="rules" :model="FData?.row">
              <el-form-item label="名称" prop="name">
                <el-input v-model="FData.row!.name" placeholder="请填写名称" clearable></el-input>
              </el-form-item>
              <el-form-item label="图片" prop="imgUrl">
                <UploadImg v-model:image-url="FData.row!.imgUrl" width="135px" height="135px" :file-size="3" />
              </el-form-item>
              <el-form-item label="价格" prop="price">
                <el-input-number v-model="FData.row!.price" :min="0" :precision="2" />
              </el-form-item>
              <el-form-item label="折扣" prop="discount">
                <el-input-number v-model="FData.row!.discount" :min="0" :max="1" :step="0.01" :precision="2" />
              </el-form-item>
              <!-- <el-form-item label="状态">
                <el-switch
                  v-model="FData.row!.status"
                  active-text="开启"
                  inactive-text="关闭"
                  inline-prompt
                  size="large"
                  :active-value="0"
                  :inactive-value="1"
                />
              </el-form-item> -->
              <el-form-item label="权益列表">
                <div class="formFlex">
                  <el-table
                    border
                    :data="FData.row.quanyiJson"
                    :header-cell-style="{ 'text-align': 'center' }"
                    :cell-style="{ 'text-align': 'center' }"
                  >
                    <el-table-column label="图标" width="100">
                      <template #default="scope">
                        <UploadImg v-model:image-url="scope.row!.img" width="80px" height="80px" :file-size="3" />
                      </template>
                    </el-table-column>
                    <el-table-column label="名称" width="150">
                      <template #default="scope">
                        <el-input v-model="scope.row!.text" placeholder="请填写名称"></el-input>
                      </template>
                    </el-table-column>
                    <el-table-column label="操作" width="150">
                      <template #header>
                        <el-button type="primary" @click="FData.row.quanyiJson.push({ text: '', img: '' })"> 新增 </el-button>
                      </template>
                      <template #default="scope">
                        <el-button type="danger" @click="FData.row.quanyiJson.splice(scope.$index, 1)"> 删除 </el-button>
                      </template>
                    </el-table-column>
                  </el-table>
                </div>
              </el-form-item>
            </el-form>
          </div>
        </el-tab-pane>
        <el-tab-pane label="权益规则">
          <WangEditor v-model:value="FData.row.contents" height="500px" />
        </el-tab-pane>
      </el-tabs>

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
import WangEditor from "@/components/WangEditor/index.vue";
import UploadImg from "@/components/Upload/Img.vue";

const dialogShow = ref(false);
const FData = ref<FProps>({
  title: "",
  row: {}
});
const rules = reactive({
  name: [{ required: true, message: "请填写名称" }],
  imgUrl: [{ required: true, message: "请上传图片" }]
});
// 提交数据（新增/编辑）
const ruleFormRef = ref<FormInstance>();
const handleSubmit = () => {
  ruleFormRef.value!.validate(async valid => {
    if (!valid) return;
    try {
      const isNotEmpty = obj => Object.values(obj).every(value => value === "");
      const quanyiJsonNotEmpty = FData.value?.row.quanyiJson.every(isNotEmpty);
      if (quanyiJsonNotEmpty) {
        ElMessage.error("权益不能为空!");
        return;
      }
      let data = {
        ...FData.value?.row,
        quanyiJson: JSON.stringify(FData.value?.row.quanyiJson)
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
