<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData?.title + '角色'" width="50%" align-center>
      <el-form ref="ruleFormRef" label-width="100px" label-suffix=" :" :rules="rules" :model="FData?.row">
        <el-form-item label="角色名称" prop="roleName">
          <el-input v-model="FData.row!.roleName" placeholder="请填写角色名称" clearable></el-input>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="FData.row!.remark" type="textarea" placeholder="请填写备注" clearable></el-input>
        </el-form-item>
        <div style="max-height: 50vh; overflow: auto">
          <el-form-item label="权限">
            <el-tree
              :default-checked-keys="FData.row!.menuIds"
              :data="SysMenuTreeList"
              show-checkbox
              node-key="Id"
              :props="{ children: 'children', label: 'title' }"
              default-expand-all
              check-strictly
              @check="currentChecked"
            />
          </el-form-item>
        </div>
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
import { sysMenuApi } from "@/api/api";
const dialogShow = ref(false);
const FData = ref<FProps>({
  title: "",
  row: {}
});
const SysMenuTreeList = ref<any>([]);
const rules = reactive({
  roleName: [{ required: true, message: "请填写角色名称" }]
});
let checkedKeys: any = [];
const currentChecked = (nodeObj: any, SelectedObj: any) => {
  checkedKeys = SelectedObj.checkedKeys;
};
// 提交数据（新增/编辑）
const ruleFormRef = ref<FormInstance>();
const handleSubmit = () => {
  ruleFormRef.value!.validate(async valid => {
    if (!valid) return;
    try {
      let data = {
        ...FData.value?.row,
        menuIds: checkedKeys
      };
      await FData.value?.api!(data);
      ElMessage.success({ message: `${FData.value?.title}角色成功！` });
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
  const { data } = await sysMenuApi.TreeList();
  // 递归处里角色
  const transformMenuTree = dataTemp => {
    return dataTemp.map(item => {
      const transformedItem = {
        ...item,
        title: item.meta.title
      };

      if (item.children && item.children.length > 0) {
        transformedItem.children = transformMenuTree(item.children);
      }

      return transformedItem;
    });
  };
  SysMenuTreeList.value = transformMenuTree(data);
  checkedKeys = FData.value.row!.menuIds;
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
