<template>
  <div>
    <el-dialog v-model="dialogShow" :title="FData.row.nickName + '用户团队'" width="50%" align-center>
      <div class="dialogScrollbar">
        <el-table :data="tableData" border style="width: 100%" row-key="Id" @expand-change="expandChange">
          <el-table-column type="expand">
            <template #default="scope">
              <el-table :data="scope.row.childrens" border>
                <el-table-column label="用户Id" prop="Id" />
                <el-table-column label="用户名称" prop="nickName" />
                <el-table-column label="手机号" prop="mobile" />
              </el-table>
            </template>
          </el-table-column>
          <el-table-column label="用户Id" prop="Id" />
          <el-table-column label="用户名称" prop="nickName" />
          <el-table-column label="手机号" prop="mobile" />
        </el-table>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogShow = false">取消</el-button>
          <el-button type="primary" @click="dialogShow = false"> 确定 </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { ElDialog } from "element-plus";
import { sysUserInfoApi } from "@/api/api";
const dialogShow = ref(false);
const FData = ref<any>({
  row: {}
});
const tableData = ref<any[]>([]);
// 接收父组件传过来的参数
const acceptParams = async (params: any) => {
  FData.value = { ...params };
  sysUserInfoApi.list({ userId: FData.value.row.Id, pageNum: 1, pageSize: 1000 }).then((res: any) => {
    tableData.value = res.data.list;
  });
  dialogShow.value = true;
};
const expandChange = (row: any) => {
  if (row.childrens) {
    return;
  }
  sysUserInfoApi.list({ userId: row.Id, pageNum: 1, pageSize: 1000 }).then((res: any) => {
    row.childrens = res.data.list;
  });
};
defineExpose({
  acceptParams
});
</script>
