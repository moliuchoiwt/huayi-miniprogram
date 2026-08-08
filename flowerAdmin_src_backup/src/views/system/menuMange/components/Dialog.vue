<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.title + '菜单'" width="50%" align-center>
      <el-form ref="ruleFormRef" label-width="140px" label-suffix=" :" :rules="rules" :model="FData?.row">
        <el-form-item label="菜单分类" prop="menuType">
          <el-radio-group v-model="FData.row!.menuType">
            <el-radio-button :label="item" :value="index" v-for="(item, index) in ['目录', '菜单', '按钮']" :key="index" />
          </el-radio-group>
        </el-form-item>
        <el-form-item label="路由地址" prop="path">
          <el-input v-model="FData.row!.path" placeholder="请填写路由地址" clearable></el-input>
        </el-form-item>
        <el-form-item label="路由name" prop="name">
          <el-input v-model="FData.row!.name" placeholder="请填写路由name" clearable></el-input>
        </el-form-item>
        <el-form-item label="路由名称" prop="title">
          <el-input v-model="FData.row!.title" placeholder="请填写路由名称" clearable></el-input>
        </el-form-item>
        <el-form-item label="视图文件路径" prop="component">
          <el-input v-model="FData.row!.component" placeholder="请填写视图文件路径" clearable></el-input>
        </el-form-item>
        <el-form-item label="外链路径">
          <el-input v-model="FData.row!.isLink" placeholder="请填写外链路径" clearable></el-input>
        </el-form-item>
        <el-form-item label="高亮父级Path">
          <el-input v-model="FData.row!.activeMenu" placeholder="请填写高亮父级Path" clearable></el-input>
        </el-form-item>
        <el-form-item label="是否缓存当前路由">
          <el-switch v-model="FData.row!.isKeepAlive" />
        </el-form-item>
        <div class="formFlex">
          <el-form-item label="是否全屏">
            <el-switch v-model="FData.row!.isFull" />
          </el-form-item>
          <el-form-item label="是否固定标签页">
            <el-switch v-model="FData.row!.isAffix" />
          </el-form-item>
          <el-form-item label="是否隐藏菜单">
            <el-switch v-model="FData.row!.isHide" />
          </el-form-item>
        </div>
        <el-form-item label="重定向路径" prop="redirect">
          <el-input v-model="FData.row!.redirect" placeholder="请填写重定向路径" clearable></el-input>
        </el-form-item>
        <el-form-item label="排序" prop="sort">
          <el-input-number v-model="FData.row!.sort" :min="0" />
        </el-form-item>
        <el-form-item label="路由图标" prop="icon">
          <SelectIcon v-model:icon-value="FData.row!.icon" />
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

<script setup lang="ts" name="MenuDialog">
import { reactive, ref } from "vue";
import { ElDialog, ElMessage, FormInstance } from "element-plus";
import SelectIcon from "@/components/SelectIcon/index.vue";
const dialogShow = ref(false);
const FData = ref<FProps>({
  title: "",
  row: {}
});
const rules = reactive({
  path: [{ required: true, message: "请填写路由地址" }],
  title: [{ required: true, message: "请填写路由name" }],
  name: [{ required: true, message: "请填写路由名称" }],
  component: [{ required: true, message: "请填写视图文件路径" }]
});

// 提交数据（新增/编辑）
const ruleFormRef = ref<FormInstance>();
const handleSubmit = () => {
  ruleFormRef.value!.validate(async valid => {
    if (!valid) return;
    try {
      await FData.value?.api!(FData.value?.row);
      ElMessage.success({ message: `${FData.value?.title}菜单成功！` });
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
const acceptParams = (params: FProps) => {
  FData.value = { ...params };
  if (params.row?.meta) {
    FData.value.row = {
      ...params.row,
      ...params.row.meta
    };
    delete FData.value.row.meta;
    delete FData.value.row.children;
  }
  dialogShow.value = true;
};

defineExpose({
  acceptParams
});
</script>

<style lang="scss" scoped></style>
