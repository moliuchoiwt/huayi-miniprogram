<template>
	<view class="page">
		<view class="form">
			<up-form labelPosition="left" label-width="140rpx">
				<up-form-item label="姓名">
					<up-input v-model="query.title" inputAlign="right" placeholder="请填写姓名" border="none"></up-input>
				</up-form-item>
				<up-form-item label="联系方式">
					<up-input v-model="query.contact" inputAlign="right" type="number" placeholder="请填写联系电话"
						border="none"></up-input>
				</up-form-item>
				<up-form-item label="意见反馈" labelPosition='top'>
					<view style="padding: 32rpx 0;width: 100%;">
						<up-textarea v-model="query.contents" placeholder="请填写您想反馈的具体问题..."></up-textarea>
					</view>
				</up-form-item>
			</up-form>
		</view>
		<view class="btns">
			<view class="btn" @click="$u.throttle(submit, 1000)">提交</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			name: '',
			query: {
				title: "",
				contact: "",
				contents: ""
			}
		}
	},
	onLoad() {
		this.query = {
			title: this.storeUser.user.nickName,
			contact: this.storeUser.user.mobile,
			contents: ""
		}
	},
	methods: {
		submit() {
			this.request('User/OperationFeedback', this.query, {}, false, true).then(res => {
				this.toast('提交成功')
				setTimeout(() => {
					uni.navigateBack()
				}, 400)
			})
		}
	}
}
</script>

<style lang="scss" scoped>
.page {
	min-height: 100vh;
	background: white;
}

.btns {
	display: flex;
	justify-content: center;

	.btn {
		width: 331rpx;
		text-align: center;
		padding: 24rpx 130rpx;
		font-size: 36rpx;
		color: #FFFFFF;
		background: #8C4FFF;
		border-radius: 16rpx;
	}
}

.form {
	background: white;
	padding: 32rpx;
}
</style>